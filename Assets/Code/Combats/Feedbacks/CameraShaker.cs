using Code.CoreSystem.Bus;
using Code.GameEvents.Feedbacks;
using Unity.Cinemachine;
using UnityEngine;

namespace Code.Combats.Feedbacks
{
    /// <summary>
    /// 셰이크 요청을 받아 Cinemachine Impulse로 카메라를 흔듭니다.
    /// </summary>
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CameraShaker : MonoBehaviour
    {
        private CinemachineImpulseSource _impulseSource;

        private void Awake()
            => _impulseSource = GetComponent<CinemachineImpulseSource>();

        private void OnEnable()
            => Bus<CameraShakeEvent>.RegisterForAll(HandleShake);

        private void OnDisable()
            => Bus<CameraShakeEvent>.UnRegisterForAll(HandleShake);

        private void HandleShake(CameraShakeEvent evt)
        {
            if (evt.Data == null)
                return;

            _impulseSource.ImpulseDefinition.ImpulseDuration = evt.Data.Duration;
            _impulseSource.GenerateImpulseWithForce(evt.Data.Force);
        }
    }
}
