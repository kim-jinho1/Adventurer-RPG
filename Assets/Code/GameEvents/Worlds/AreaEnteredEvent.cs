using Code.CoreSystem.Bus;
using Code.Worlds;

namespace Code.GameEvents.Worlds
{
    /// <summary>
    /// 지역에 들어와 화면이 다 밝아졌다는 알림입니다. 자동 저장 · 배경음 · 지역 이름 표시가 받습니다.
    /// </summary>
    public readonly struct AreaEnteredEvent : IEvent
    {
        public AreaSO Area { get; }

        public AreaEnteredEvent(AreaSO area)
        {
            Area = area;
        }
    }
}
