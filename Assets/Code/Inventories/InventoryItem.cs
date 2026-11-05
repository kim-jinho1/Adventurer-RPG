using Code.Items;
using UnityEngine;

namespace Code.Inventories
{
    /// <summary>
    /// 인벤토리 한 칸에 들어 있는 아이템과 개수입니다.
    /// </summary>
    public class InventoryItem : IReadOnlyInventoryItem
    {
        public AbstractItemDataSO ItemData { get; }
        public int StackSize { get; private set; }

        /// <summary> 이 칸에 더 넣을 수 있는 개수입니다. </summary>
        public int RemainSpace
            => ItemData.MaxStack - StackSize;

        public bool IsFull
            => StackSize >= ItemData.MaxStack;

        public bool IsEmpty
            => StackSize <= 0;

        public InventoryItem(AbstractItemDataSO itemData, int stackSize = 0)
        {
            ItemData = itemData;
            StackSize = stackSize;
        }

        /// <summary>
        /// 개수를 더합니다. 최대 스택을 넘친 양을 반환합니다.
        /// </summary>
        public int AddStack(int amount)
        {
            int added = Mathf.Clamp(amount, 0, RemainSpace);
            StackSize += added;
            return amount - added;
        }

        /// <summary>
        /// 개수를 뺍니다. 실제로 뺀 양을 반환합니다.
        /// </summary>
        public int RemoveStack(int amount)
        {
            int removed = Mathf.Clamp(amount, 0, StackSize);
            StackSize -= removed;
            return removed;
        }
    }
}
