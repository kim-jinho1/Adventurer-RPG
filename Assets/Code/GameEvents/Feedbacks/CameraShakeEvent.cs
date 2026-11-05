using Code.Combats.Feedbacks;
using Code.CoreSystem.Bus;

namespace Code.GameEvents.Feedbacks
{
    /// <summary>
    /// 카메라를 흔들어 달라는 요청입니다.
    /// </summary>
    public readonly struct CameraShakeEvent : IEvent
    {
        public ShakeDataSO Data { get; }

        public CameraShakeEvent(ShakeDataSO data)
        {
            Data = data;
        }
    }
}
