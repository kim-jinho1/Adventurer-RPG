using Code.Items;

namespace Code.Inventories
{
    /// <summary>
    /// 읽기만 할 수 있는 인벤토리 칸 인터페이스입니다.
    /// </summary>
    public interface IReadOnlyInventoryItem
    {
        /// <summary> 칸에 든 아이템 </summary>
        public AbstractItemDataSO ItemData { get; }
        /// <summary> 쌓인 개수 </summary>
        public int StackSize { get; }
    }
}
