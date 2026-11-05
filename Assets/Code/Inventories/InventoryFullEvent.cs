using Code.CoreSystem.Bus;
using Code.Items;

namespace Code.Inventories
{
    /// <summary>
    /// 가방이 차서 아이템을 받지 못했다고 알리는 버스 이벤트입니다. "가방이 가득 찼습니다" 안내 UI가 받습니다.
    /// </summary>
    public readonly struct InventoryFullEvent : IEvent
    {
        public AbstractItemDataSO Item { get; }

        public InventoryFullEvent(AbstractItemDataSO item)
        {
            Item = item;
        }
    }
}
