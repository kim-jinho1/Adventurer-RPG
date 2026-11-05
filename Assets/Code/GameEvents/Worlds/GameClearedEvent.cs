using Code.CoreSystem.Bus;

namespace Code.GameEvents.Worlds
{
    /// <summary>
    /// 마지막 보스를 쓰러뜨려 게임을 끝냈다는 알림입니다. 엔딩 화면이 받습니다.
    /// </summary>
    public readonly struct GameClearedEvent : IEvent
    {
    }
}
