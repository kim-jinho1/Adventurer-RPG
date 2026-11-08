using System;
using System.Collections.Generic;
using Code.Agents;
using Code.Equipments;
using Code.Inventories;
using Code.Items;
using Code.Modules;
using UnityEngine;

namespace Code.UIs
{
    /// <summary>
    /// 3탭 격자 인벤토리 + 장비 슬롯 창입니다. 칸을 고르면 상세 정보와 사용 · 장착 · 해제 버튼을 보여 줍니다.
    /// </summary>
    public class InventoryPanel : AbstractPanel
    {
        [Header("Reference")]
        [SerializeField] private ModuleOwner player;

        [Header("Tabs")]
        [SerializeField] private List<InventoryTabData> tabs = new();
        [SerializeField] private Color selectedTabColor = Color.white;
        [SerializeField] private Color normalTabColor = new(1f, 1f, 1f, 0.45f);

        [Header("View")]
        [SerializeField] private InventorySlotListView slotListView;
        [SerializeField] private EquipSlotListView equipSlotListView;
        [SerializeField] private ItemDetailView detailView;

        [Header("Texts")]
        [SerializeField] private string unequipLabel = "해제";     // 장비 칸을 골랐을 때 버튼 글자

        private const int _noSelection = -1;

        private IInventoryModule _inventoryModule;
        private IEquipmentModule _equipmentModule;
        private IItemUseModule _itemUseModule;
        private IHealthModule _healthModule;

        private ItemTypeEnum _currentTab = ItemTypeEnum.Material;
        private int _selectedSlotIndex = _noSelection;
        private bool _isEquipSlotSelected;
        private EquipSlotEnum _selectedEquipSlot;

        // 플레이어 모듈은 플레이어 Awake에서 초기화되므로 Start에서 가져온다
        private void Start()
        {
            Debug.Assert(player != null, $"[InventoryPanel] player 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(slotListView != null, $"[InventoryPanel] slotListView 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(equipSlotListView != null, $"[InventoryPanel] equipSlotListView 연결 안 됨: {gameObject.name}", this);
            if (player == null || slotListView == null || equipSlotListView == null)
                return;

            _inventoryModule = player.GetModule<IInventoryModule>();
            _equipmentModule = player.GetModule<IEquipmentModule>();
            _itemUseModule = player.GetModule<IItemUseModule>();
            _healthModule = player.GetModule<IHealthModule>();
            Debug.Assert(_inventoryModule != null, $"[InventoryPanel] player에 IInventoryModule 모듈 없음: {gameObject.name}", this);

            CreateSlotViews();
            BindEvents();
        }

        protected override void OnDestroy()
        {
            UnbindEvents();
            base.OnDestroy();
        }

        /// <summary> 창을 열거나 닫습니다. </summary>
        public void Toggle()
            => SetOpen(!IsOpen);

        protected override void HandleOpened()
        {
            ClearSelection();
            RefreshAll();
        }

        private void CreateSlotViews()
        {
            if (_inventoryModule == null)
                return;

            // 탭마다 칸 수가 같지만, 달라져도 가장 큰 탭에 맞춰 만들고 나머지는 숨긴다
            int slotCount = 0;
            foreach (ItemTypeEnum itemType in Enum.GetValues(typeof(ItemTypeEnum)))
                slotCount = Mathf.Max(slotCount, _inventoryModule.GetSlotCount(itemType));

            slotListView.Build(slotCount);
        }

        private void BindEvents()
        {
            if (CharacterInput != null)
                CharacterInput.OnInventoryKeyPressed += HandleInventoryKey;

            foreach (InventoryTabData tab in tabs)
            {
                if (tab.Button != null)
                    tab.Button.onClick.AddListener(() => HandleTabClicked(tab.ItemType));
            }

            slotListView.OnSlotSelected += HandleSlotSelected;
            equipSlotListView.OnEquipSlotSelected += HandleEquipSlotSelected;

            if (detailView != null)
                detailView.OnActionClicked += HandleActionClicked;

            if (_inventoryModule != null)
                _inventoryModule.OnInventoryChanged += HandleInventoryChanged;

            if (_equipmentModule != null)
                _equipmentModule.OnEquipmentChanged += HandleEquipmentChanged;

            if (_healthModule != null)
                _healthModule.OnHealthChange += HandleHealthChange;
        }

        private void UnbindEvents()
        {
            if (CharacterInput != null)
                CharacterInput.OnInventoryKeyPressed -= HandleInventoryKey;

            // 탭 버튼은 람다로 등록해 하나씩 해제할 수 없어 통째로 지운다 (이 창 전용 버튼)
            foreach (InventoryTabData tab in tabs)
            {
                if (tab.Button != null)
                    tab.Button.onClick.RemoveAllListeners();
            }

            if (slotListView != null)
                slotListView.OnSlotSelected -= HandleSlotSelected;

            if (equipSlotListView != null)
                equipSlotListView.OnEquipSlotSelected -= HandleEquipSlotSelected;

            if (detailView != null)
                detailView.OnActionClicked -= HandleActionClicked;

            if (_inventoryModule != null)
                _inventoryModule.OnInventoryChanged -= HandleInventoryChanged;

            if (_equipmentModule != null)
                _equipmentModule.OnEquipmentChanged -= HandleEquipmentChanged;

            if (_healthModule != null)
                _healthModule.OnHealthChange -= HandleHealthChange;
        }

        private void RefreshAll()
        {
            RefreshTabs();
            RefreshSlots();
            RefreshEquipSlots();
            RefreshDetail();
        }

        private void RefreshTabs()
        {
            foreach (InventoryTabData tab in tabs)
            {
                if (tab.Background != null)
                    tab.Background.color = tab.ItemType == _currentTab ? selectedTabColor : normalTabColor;
            }
        }

        private void RefreshSlots()
        {
            if (_inventoryModule == null)
                return;

            // 장비 칸을 골랐으면 가방 칸에는 선택 테두리를 그리지 않는다
            slotListView.Show(_inventoryModule.GetSlots(_currentTab), _isEquipSlotSelected ? _noSelection : _selectedSlotIndex);
        }

        private void RefreshEquipSlots()
            => equipSlotListView.Show(_equipmentModule, _isEquipSlotSelected, _selectedEquipSlot);

        private void RefreshDetail()
        {
            if (detailView == null)
                return;

            if (_isEquipSlotSelected)
            {
                EquipItemDataSO equipped = _equipmentModule?.GetEquippedItem(_selectedEquipSlot);
                // 필수 슬롯(무기)이거나 가방에 자리가 없으면 벗을 수 없으므로 버튼을 막아 눌러도 반응 없는 상황을 없앤다
                bool canUnequip = _equipmentModule != null && _equipmentModule.CanUnequip(_selectedEquipSlot);
                detailView.Show(equipped, unequipLabel, canUnequip);
                return;
            }

            IReadOnlyInventoryItem slot = _inventoryModule?.GetSlot(_currentTab, _selectedSlotIndex);
            if (slot == null)
            {
                detailView.Clear();
                return;
            }

            // 버튼 글자는 아이템이, 누를 수 있는지는
            // 아이템 쓰기 모듈이 정한다
            string actionLabel = slot.ItemData is IItemAction action ? action.ActionLabel : string.Empty;
            bool canAct = _itemUseModule != null && _itemUseModule.CanActAt(_currentTab, _selectedSlotIndex);
            detailView.Show(slot.ItemData, actionLabel, canAct);
        }

        private void ClearSelection()
        {
            _selectedSlotIndex = _noSelection;
            _isEquipSlotSelected = false;
        }

        private void HandleInventoryKey()
        {
            // 타이틀 · 엔딩 · 제작 창 같은 다른 화면이 입력을 막고 있으면 그 위에 가방을 열지 않는다
            if (!IsOpen && CharacterInput != null && CharacterInput.IsGameplayBlocked)
                return;

            Toggle();
        }

        private void HandleTabClicked(ItemTypeEnum itemType)
        {
            _currentTab = itemType;
            ClearSelection();
            RefreshAll();
        }

        private void HandleSlotSelected(int index)
        {
            _isEquipSlotSelected = false;
            _selectedSlotIndex = index;
            RefreshSlots();
            RefreshEquipSlots();
            RefreshDetail();
        }

        private void HandleEquipSlotSelected(EquipSlotEnum slot)
        {
            _isEquipSlotSelected = true;
            _selectedEquipSlot = slot;
            _selectedSlotIndex = _noSelection;
            RefreshSlots();
            RefreshEquipSlots();
            RefreshDetail();
        }

        private void HandleActionClicked()
        {
            if (_isEquipSlotSelected)
            {
                _equipmentModule?.Unequip(_selectedEquipSlot);
                return;
            }

            // 장착은 칸을 비웠다 다시 채우며 가방 변경을 두 번 알려 중간에 선택이 지워지므로, 끝난 뒤 되살린다
            int slotIndex = _selectedSlotIndex;
            if (_itemUseModule == null || !_itemUseModule.ActAt(_currentTab, slotIndex))
                return;

            if (_inventoryModule.GetSlot(_currentTab, slotIndex) == null)
                return;

            _selectedSlotIndex = slotIndex;
            RefreshSlots();
            RefreshDetail();
        }

        private void HandleInventoryChanged(ItemTypeEnum itemType)
        {
            if (!IsOpen || itemType != _currentTab)
                return;

            // 마지막 포션을 쓰거나 장비를 장착해 고른 칸이 비면 선택 테두리도 지운다
            if (!_isEquipSlotSelected && _inventoryModule.GetSlot(_currentTab, _selectedSlotIndex) == null)
                _selectedSlotIndex = _noSelection;

            RefreshSlots();
            RefreshDetail();
        }

        private void HandleEquipmentChanged(EquipSlotEnum slot, EquipItemDataSO prevItem, EquipItemDataSO currentItem)
        {
            if (!IsOpen)
                return;

            RefreshEquipSlots();
            RefreshDetail();
        }

        // 체력에 따라 포션 사용 가능 여부가 바뀌므로 버튼 상태를 다시 계산한다
        private void HandleHealthChange(float before, float health, float max)
        {
            if (IsOpen)
                RefreshDetail();
        }
    }
}
