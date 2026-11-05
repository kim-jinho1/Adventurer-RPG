using Code.CoreSystem.Bus;

namespace Code.Levels
{
    /// <summary>
    /// 경험치를 얻었다고 알리는 버스 이벤트입니다. 적 처치처럼 받을 대상을 모르는 쪽이 보냅니다.
    /// </summary>
    public readonly struct ExpGainEvent : IEvent
    {
        public int Amount { get; }

        public ExpGainEvent(int amount)
        {
            Amount = amount;
        }
    }
}
