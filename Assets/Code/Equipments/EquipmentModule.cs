using System;
using System.Collections.Generic;
using Code.Agents.Stats;
using Code.GameSystems.Saves;
using Code.Inventories;
using Code.Items;
using Code.Modules;
using UnityEngine;

namespace Code.Equipments
{
    /// <summary>
    /// 무기 · 방어구를 장착하고, 장착하는 즉시 스탯 모디파이어로 보너스를 반영합니다.
    /// </summary>
    public class EquipmentModule : MonoBehaviour, IModule, IAfterInitModule, IEquipmentModule, ISaveable
    {
        /// <summary>
        /// 저장 파일에 쓰는 장비 데이터입니다. 끼고 있는 장비의 AssetIndex 목록을 담습니다.
        /// </summary>
        [Serializable]
        private struct EquipmentSaveData
        {
            public List<int> AssetIndices;
        }

        // 가방을 거치지 않고 처음부터 끼고 시작하는 장비 (예: 목검)
        [SerializeField] private List<EquipItemDataSO> startEquipments = new();
        [SerializeField] private EquipSlotTableSO slotTable; // 슬롯마다 비울 수 있는지 정한 표

        [field: Header("Save")]
        [field: SerializeField] public SaveIdSO SaveId { get; private set; }
        [SerializeField] private ItemTableSO itemTable;     // 저장된 AssetIndex를 장비로 되돌릴 때 쓴다

        public event IEquipmentModule.EquipmentChangeHandler OnEquipmentChanged;

        private readonly Dictionary<EquipSlotEnum, EquipItemDataSO> _equippedItems = new();

        private IStatModule _statModule;
        private IInventoryModule _inventoryModule;

        public void Initialize(ModuleOwner owner)
        {
            _statModule = owner.GetModule<IStatModule>();
            _inventoryModule = owner.GetModule<IInventoryModule>();
            Debug.Assert(_statModule != null, $"[EquipmentModule] IStatModule 모듈 없음: {gameObject.name}", this);
            Debug.Assert(_inventoryModule != null, $"[EquipmentModule] IInventoryModule 모듈 없음: {gameObject.name}", this);
            Debug.Assert(slotTable != null, $"[EquipmentModule] slotTable 연결 안 됨: {gameObject.name}", this);
        }

        // 메쉬 모듈이 Initialize에서 구독을 마친 뒤에 장착해야 시작 장비 모습도 반영된다
        public void AfterInitialize()
        {
            foreach (EquipItemDataSO item in startEquipments)
            {
                if (item == null || GetEquippedItem(item.EquipSlot) != null)
                    continue;

                _equippedItems[item.EquipSlot] = item;
                item.ApplyStats(_statModule);
                OnEquipmentChanged?.Invoke(item.EquipSlot, null, item);
            }
        }

        public EquipItemDataSO GetEquippedItem(EquipSlotEnum slot)
            => _equippedItems.GetValueOrDefault(slot);

        public bool IsEquipped(EquipItemDataSO item)
            => item != null && GetEquippedItem(item.EquipSlot) == item;

        // 공격이 무기를 든 상태를 전제로 하므로 무기는 벗을 수 없고 교체만 된다
        public bool CanUnequip(EquipSlotEnum slot)
        {
            EquipItemDataSO item = GetEquippedItem(slot);
            return !slotTable.IsRequired(slot) && item != null && _inventoryModule.CanAddItem(item);
        }

        public bool EquipAt(int slotIndex)
        {
            IReadOnlyInventoryItem slot = _inventoryModule.GetSlot(ItemTypeEnum.Equipment, slotIndex);
            if (slot?.ItemData is not EquipItemDataSO item || IsEquipped(item))
                return false;

            if (!_inventoryModule.RemoveItemAt(ItemTypeEnum.Equipment, slotIndex))
                return false;

            EquipItemDataSO prevItem = GetEquippedItem(item.EquipSlot);
            if (prevItem != null)
            {
                // 꺼낸 칸이 비어 보통은 자리가 있지만, 없으면 꺼낸 장비를 되돌리고 취소한다
                if (!_inventoryModule.CanAddItem(prevItem))
                {
                    _inventoryModule.AddItem(item);
                    return false;
                }

                prevItem.RemoveStats(_statModule);
                _inventoryModule.AddItem(prevItem);
            }

            _equippedItems[item.EquipSlot] = item;
            item.ApplyStats(_statModule);
            OnEquipmentChanged?.Invoke(item.EquipSlot, prevItem, item);
            return true;
        }

        public bool Unequip(EquipSlotEnum slot)
        {
            if (!CanUnequip(slot))
                return false;

            EquipItemDataSO item = GetEquippedItem(slot);
            _equippedItems.Remove(slot);
            item.RemoveStats(_statModule);
            _inventoryModule.AddItem(item);
            OnEquipmentChanged?.Invoke(slot, item, null);
            return true;
        }

        public string GetSaveData()
        {
            List<int> assetIndices = new();
            foreach (EquipItemDataSO item in _equippedItems.Values)
                assetIndices.Add(item.AssetIndex);

            return JsonUtility.ToJson(new EquipmentSaveData { AssetIndices = assetIndices });
        }

        public void RestoreData(string data)
        {
            Debug.Assert(itemTable != null, $"[EquipmentModule] itemTable 연결 안 됨: {gameObject.name}", this);
            EquipmentSaveData saveData = JsonUtility.FromJson<EquipmentSaveData>(data);

            Dictionary<EquipSlotEnum, EquipItemDataSO> savedItems = new();
            if (saveData.AssetIndices != null && itemTable != null)
                foreach (int assetIndex in saveData.AssetIndices)
                    if (itemTable.TryGetItemData(assetIndex, out AbstractItemDataSO item) && item is EquipItemDataSO equipItem)
                        savedItems[equipItem.EquipSlot] = equipItem;

            // 시작 장비를 이미 끼고 있으므로 가방을 거치지 않고 슬롯마다 저장된 장비로 바꿔 끼운다
            foreach (EquipSlotEnum slot in Enum.GetValues(typeof(EquipSlotEnum)))
            {
                EquipItemDataSO prevItem = GetEquippedItem(slot);
                EquipItemDataSO nextItem = savedItems.GetValueOrDefault(slot);

                // 필수 슬롯은 비울 수 없으니, 저장된 장비를 못 찾으면 지금 장비를 그대로 둔다
                if (prevItem == nextItem || (slotTable.IsRequired(slot) && nextItem == null))
                    continue;

                if (prevItem != null)
                    prevItem.RemoveStats(_statModule);

                if (nextItem != null)
                {
                    _equippedItems[slot] = nextItem;
                    nextItem.ApplyStats(_statModule);
                }
                else
                {
                    _equippedItems.Remove(slot);
                }

                OnEquipmentChanged?.Invoke(slot, prevItem, nextItem);
            }
        }
    }
}
