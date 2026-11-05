using Code.CoreSystem.Bus;

namespace Code.GameEvents.Worlds
{
    /// <summary>
    /// 지금 지역을 떠나기 직전 알림입니다. 화면이 가려진 뒤, 지역 씬을 내리기 전에 보냅니다.
    /// </summary>
    public readonly struct AreaLeavingEvent : IEvent
    { }
}
