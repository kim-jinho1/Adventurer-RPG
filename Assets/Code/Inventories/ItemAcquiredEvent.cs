using Code.CoreSystem.Bus;
using Code.Items;

namespace Code.Inventories
{
    /// <summary>
    /// 줍기 · 채집으로 아이템을 얻었다고 알리는 버스 이벤트입니다. 획득 알림 UI가 받습니다.
    /// </summary>
    public readonly struct ItemAcquiredEvent : IEvent
    {
        public AbstractItemDataSO Item { get; }
        public int Count { get; }

        public ItemAcquiredEvent(AbstractItemDataSO item, int count)
        {
            Item = item;
            Count = count;
        }
    }
}
