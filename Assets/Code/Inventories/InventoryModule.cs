using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Code.GameSystems.Saves;
using Code.Items;
using Code.Modules;
using UnityEngine;

namespace Code.Inventories
{
    /// <summary>
    /// 아이템 종류(탭)마다 고정 칸 수를 가진 격자형 인벤토리 모듈입니다. 데이터만 다루고 UI는 OnInventoryChanged를 구독해 그립니다.
    /// </summary>
    public class InventoryModule : MonoBehaviour, IModule, IAfterInitModule, IInventoryModule, ISaveable
    {
        /// <summary>
        /// 저장 파일에 쓰는 가방 칸 하나의 데이터입니다.
        /// </summary>
        [Serializable]
        private struct SlotSaveData
        {
            public ItemTypeEnum ItemType;
            public int SlotIndex;
            public int AssetIndex;
            public int Count;
        }

        /// <summary>
        /// 저장 파일에 쓰는 가방 데이터입니다. 빈 칸은 담지 않습니다.
        /// </summary>
        [Serializable]
        private struct InventorySaveData
        {
            public List<SlotSaveData> Slots;
        }

        [Header("Inventory Setting")]
        [SerializeField, Min(1)] private int slotCountPerTab = 30;

        [Header("Start Items")]
        [SerializeField] private List<ItemAmount> startItems = new();

        [field: Header("Save")]
        [field: SerializeField] public SaveIdSO SaveId { get; private set; }
        [SerializeField] private ItemTableSO itemTable;     // 저장된 AssetIndex를 아이템으로 되돌릴 때 쓴다

        public event IInventoryModule.InventoryChangeHandler OnInventoryChanged;

        private readonly Dictionary<ItemTypeEnum, InventoryItem[]> _slotsByType = new();
        // 밖에는 배열 대신 이 포장을 줘서, 원본 배열을 그대로 비추되 형변환으로 칸을 갈아 끼울 수 없게 한다
        private readonly Dictionary<ItemTypeEnum, ReadOnlyCollection<InventoryItem>> _readOnlySlotsByType = new();
        // CanAddItems가 호출마다 새로 만들지 않도록 재사용한다
        private readonly Dictionary<AbstractItemDataSO, int> _countByItem = new();
        private readonly Dictionary<ItemTypeEnum, int> _neededSlotsByType = new();

        public void Initialize(ModuleOwner owner)
        {
            // 종류가 늘어도 탭이 자동으로 생기도록 열거형 전체를 돈다
            foreach (ItemTypeEnum itemType in Enum.GetValues(typeof(ItemTypeEnum)))
            {
                InventoryItem[] slots = new InventoryItem[slotCountPerTab];
                _slotsByType[itemType] = slots;
                _readOnlySlotsByType[itemType] = Array.AsReadOnly(slots);
            }
        }

        public void AfterInitialize()
        {
            foreach (ItemAmount startItem in startItems)
            {
                if (startItem.Item != null)
                    AddItem(startItem.Item, startItem.Count);
            }
        }

        public int GetSlotCount(ItemTypeEnum itemType)
            => _slotsByType[itemType].Length;

        public IReadOnlyList<IReadOnlyInventoryItem> GetSlots(ItemTypeEnum itemType)
            => _readOnlySlotsByType[itemType];

        public IReadOnlyInventoryItem GetSlot(ItemTypeEnum itemType, int slotIndex)
            => IsValidSlot(itemType, slotIndex) ? _slotsByType[itemType][slotIndex] : null;

        public int GetItemCount(AbstractItemDataSO item)
        {
            if (item == null)
                return 0;

            int count = 0;
            foreach (InventoryItem slot in _slotsByType[item.ItemType])
            {
                if (slot != null && slot.ItemData == item)
                    count += slot.StackSize;
            }

            return count;
        }

        public bool HasItem(AbstractItemDataSO item, int count = 1)
            => GetItemCount(item) >= count;

        public bool CanAddItem(AbstractItemDataSO item, int count = 1)
        {
            if (item == null || count <= 0)
                return false;

            return GetNeededEmptySlots(item, count) <= CountEmptySlots(item.ItemType);
        }

        public bool CanAddItems(IReadOnlyList<ItemAmount> items)
        {
            _countByItem.Clear();
            _neededSlotsByType.Clear();

            // 같은 아이템이 여러 줄이면 덜 찬 칸 자리를 두 번 쓰지 않도록 먼저 합친다
            foreach (ItemAmount itemAmount in items)
            {
                if (itemAmount.Item == null)
                    return false;

                if (itemAmount.Count <= 0)
                    continue;

                _countByItem[itemAmount.Item] = _countByItem.GetValueOrDefault(itemAmount.Item) + itemAmount.Count;
            }

            // 아이템마다 따로 빈 칸을 세면 같은 빈 칸을 여러 번 노리므로, 탭별로 필요한 빈 칸을 더해 한 번에 비교한다
            foreach (KeyValuePair<AbstractItemDataSO, int> pair in _countByItem)
            {
                ItemTypeEnum itemType = pair.Key.ItemType;
                _neededSlotsByType[itemType] = _neededSlotsByType.GetValueOrDefault(itemType) + GetNeededEmptySlots(pair.Key, pair.Value);
            }

            foreach (KeyValuePair<ItemTypeEnum, int> pair in _neededSlotsByType)
            {
                if (pair.Value > CountEmptySlots(pair.Key))
                    return false;
            }

            return true;
        }

        public int AddItem(AbstractItemDataSO item, int count = 1)
        {
            if (item == null || count <= 0)
                return Mathf.Max(count, 0);

            InventoryItem[] slots = _slotsByType[item.ItemType];
            int remain = count;

            // 칸을 아끼려고 덜 찬 같은 아이템 칸부터 채운다
            for (int i = 0; i < slots.Length && remain > 0; i++)
            {
                if (slots[i] != null && slots[i].ItemData == item && !slots[i].IsFull)
                    remain = slots[i].AddStack(remain);
            }

            for (int i = 0; i < slots.Length && remain > 0; i++)
            {
                if (slots[i] != null)
                    continue;

                slots[i] = new InventoryItem(item);
                remain = slots[i].AddStack(remain);
            }

            if (remain != count)
                OnInventoryChanged?.Invoke(item.ItemType);

            return remain;
        }

        public bool RemoveItem(AbstractItemDataSO item, int count = 1)
        {
            if (item == null || count <= 0 || !HasItem(item, count))
                return false;

            InventoryItem[] slots = _slotsByType[item.ItemType];
            int remain = count;

            // 뒤 칸부터 빼서 앞쪽 칸 배치가 덜 흐트러지게 한다
            for (int i = slots.Length - 1; i >= 0 && remain > 0; i--)
            {
                if (slots[i] == null || slots[i].ItemData != item)
                    continue;

                remain -= slots[i].RemoveStack(remain);
                if (slots[i].IsEmpty)
                    slots[i] = null;
            }

            OnInventoryChanged?.Invoke(item.ItemType);
            return true;
        }

        public bool RemoveItemAt(ItemTypeEnum itemType, int slotIndex, int count = 1)
        {
            // 개수를 바꿔야 하므로 읽기 전용 GetSlot 대신 원본 칸을 꺼낸다
            InventoryItem slot = IsValidSlot(itemType, slotIndex) ? _slotsByType[itemType][slotIndex] : null;
            if (slot == null || count <= 0 || slot.StackSize < count)
                return false;

            slot.RemoveStack(count);
            if (slot.IsEmpty)
                _slotsByType[itemType][slotIndex] = null;

            OnInventoryChanged?.Invoke(itemType);
            return true;
        }

        public void MoveItem(ItemTypeEnum itemType, int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex || !IsValidSlot(itemType, fromIndex) || !IsValidSlot(itemType, toIndex))
                return;

            InventoryItem[] slots = _slotsByType[itemType];
            InventoryItem from = slots[fromIndex];
            InventoryItem to = slots[toIndex];
            if (from == null)
                return;

            if (to != null && to.ItemData == from.ItemData && !to.IsFull)
            {
                // 같은 아이템이면 합치고, 넘친 양은 원래 칸에 남긴다
                int overflow = to.AddStack(from.StackSize);
                from.RemoveStack(from.StackSize - overflow);
                if (from.IsEmpty)
                    slots[fromIndex] = null;
            }
            else
            {
                slots[fromIndex] = to;
                slots[toIndex] = from;
            }

            OnInventoryChanged?.Invoke(itemType);
        }

        public string GetSaveData()
        {
            List<SlotSaveData> savedSlots = new();
            foreach (KeyValuePair<ItemTypeEnum, InventoryItem[]> pair in _slotsByType)
            {
                for (int i = 0; i < pair.Value.Length; i++)
                {
                    InventoryItem slot = pair.Value[i];
                    if (slot == null)
                        continue;

                    savedSlots.Add(new SlotSaveData
                    {
                        ItemType = pair.Key,
                        SlotIndex = i,
                        AssetIndex = slot.ItemData.AssetIndex,
                        Count = slot.StackSize
                    });
                }
            }

            return JsonUtility.ToJson(new InventorySaveData { Slots = savedSlots });
        }

        public void RestoreData(string data)
        {
            Debug.Assert(itemTable != null, $"[InventoryModule] itemTable 연결 안 됨: {gameObject.name}", this);
            InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(data);

            // 시작 아이템이 이미 들어가 있으므로 전부 비우고 저장된 칸 그대로 채운다
            foreach (InventoryItem[] slots in _slotsByType.Values)
                Array.Clear(slots, 0, slots.Length);

            if (saveData.Slots != null && itemTable != null)
            {
                foreach (SlotSaveData savedSlot in saveData.Slots)
                {
                    // 아이템이 지워졌거나 칸 수가 줄어든 옛 저장 파일이면 그 칸은 건너뛴다
                    if (!itemTable.TryGetItemData(savedSlot.AssetIndex, out AbstractItemDataSO item)
                        || !IsValidSlot(item.ItemType, savedSlot.SlotIndex))
                        continue;

                    InventoryItem slot = new InventoryItem(item);
                    slot.AddStack(savedSlot.Count);
                    _slotsByType[item.ItemType][savedSlot.SlotIndex] = slot;
                }
            }

            foreach (ItemTypeEnum itemType in _slotsByType.Keys)
                OnInventoryChanged?.Invoke(itemType);
        }

        private bool IsValidSlot(ItemTypeEnum itemType, int slotIndex)
            => _slotsByType.TryGetValue(itemType, out InventoryItem[] slots) && slotIndex >= 0 && slotIndex < slots.Length;

        private int CountEmptySlots(ItemTypeEnum itemType)
        {
            int count = 0;
            foreach (InventoryItem slot in _slotsByType[itemType])
                if (slot == null)
                    count++;

            return count;
        }

        // AddItem과 같은 순서로, 같은 아이템의 덜 찬 칸을 먼저 채우고 남는 양에 빈 칸이 몇 개 드는지 센다
        private int GetNeededEmptySlots(AbstractItemDataSO item, int count)
        {
            int remain = count;
            foreach (InventoryItem slot in _slotsByType[item.ItemType])
                if (slot != null && slot.ItemData == item)
                    remain -= slot.RemainSpace;

            if (remain <= 0)
                return 0;

            // 정수 나눗셈은 내림이므로 올림으로 바꾼다
            return (remain + item.MaxStack - 1) / item.MaxStack;
        }
    }
}
