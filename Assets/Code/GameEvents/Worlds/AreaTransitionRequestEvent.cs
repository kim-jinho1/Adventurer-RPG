using Code.CoreSystem.Bus;
using Code.Worlds;

namespace Code.GameEvents.Worlds
{
    /// <summary>
    /// 다른 지역으로 옮겨 달라는 요청입니다. 지역의 어느 도착 지점에 내릴지 함께 보냅니다.
    /// </summary>
    public readonly struct AreaTransitionRequestEvent : IEvent
    {
        public AreaSO Area { get; }
        public string SpawnPointId { get; }

        public AreaTransitionRequestEvent(AreaSO area, string spawnPointId)
        {
            Area = area;
            SpawnPointId = spawnPointId;
        }
    }
}
