using System.Collections.Generic;
using Code.Inventories;
using UnityEngine;

namespace Code.UIs
{
    /// <summary>
    /// 가방 창의 격자 칸 목록입니다. 칸을 그리고, 칸을 누르면 몇 번째를 골랐는지 알립니다.
    /// </summary>
    public class InventorySlotListView : MonoBehaviour
    {
        [Header("Slot List")]
        [SerializeField] private ItemSlotView slotTemplate;
        [SerializeField] private Transform slotContainer;

        /// <summary> 칸 선택 델리게이트입니다. </summary>
        public delegate void SlotSelectHandler(int index);
        /// <summary> 칸을 누르면 호출됩니다. 무엇을 할지는 이 뷰를 쓰는 창이 정합니다. </summary>
        public event SlotSelectHandler OnSlotSelected;

        private readonly List<ItemSlotView> _slotViews = new();

        private void Awake()
        {
            Debug.Assert(slotTemplate != null, $"[InventorySlotListView] slotTemplate 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(slotContainer != null, $"[InventorySlotListView] slotContainer 연결 안 됨: {gameObject.name}", this);
            if (slotTemplate != null)
                slotTemplate.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            foreach (ItemSlotView slotView in _slotViews)
            {
                if (slotView != null)
                    slotView.OnSlotClicked -= HandleSlotClicked;
            }
        }

        /// <summary>
        /// 칸을 slotCount개까지 만듭니다. 이미 있는 칸은 다시 만들지 않습니다.
        /// </summary>
        public void Build(int slotCount)
        {
            if (slotTemplate == null || slotContainer == null)
                return;

            while (_slotViews.Count < slotCount)
            {
                ItemSlotView slotView = Instantiate(slotTemplate, slotContainer);
                slotView.gameObject.SetActive(true);
                slotView.SetIndex(_slotViews.Count);
                slotView.OnSlotClicked += HandleSlotClicked;
                _slotViews.Add(slotView);
            }
        }

        /// <summary>
        /// 탭의 칸들을 그리고 고른 칸을 강조합니다. 탭 칸 수보다 많은 칸은 숨깁니다.
        /// </summary>
        public void Show(IReadOnlyList<IReadOnlyInventoryItem> slots, int selectedIndex)
        {
            for (int i = 0; i < _slotViews.Count; i++)
            {
                ItemSlotView slotView = _slotViews[i];
                bool isInRange = i < slots.Count;
                slotView.gameObject.SetActive(isInRange);
                if (!isInRange)
                    continue;

                IReadOnlyInventoryItem slot = slots[i];
                if (slot == null)
                    slotView.Clear();
                else
                    slotView.Show(slot.ItemData, slot.StackSize > 1 ? slot.StackSize.ToString() : string.Empty);

                slotView.SetSelected(i == selectedIndex);
            }
        }

        private void HandleSlotClicked(ItemSlotView slotView)
            => OnSlotSelected?.Invoke(slotView.Index);
    }
}
