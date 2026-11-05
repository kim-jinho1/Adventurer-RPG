using Code.Agents;
using Code.CoreSystem.Bus;
using Code.GameEvents.Feedbacks;
using Code.Modules;
using UnityEngine;

namespace Code.Combats.Feedbacks
{
    /// <summary>
    /// 걸을 때 발이 땅에 닿을 때마다 카메라를 흔듭니다. 덩치 큰 적(보스)에 붙여 무게감을 줍니다.
    /// 걷기 클립에 FootstepTrigger 애니메이션 이벤트가 있어야 합니다.
    /// </summary>
    public class FootstepShakeModule : MonoBehaviour, IModule
    {
        [Header("Shake Setting")]
        [SerializeField] private ShakeDataSO stepShake;
        [SerializeField, Min(0f)] private float shakeRange = 18f;     // 카메라가 이 거리 안에 있을 때만 흔든다
        [SerializeField] private OwnerEnum shakeEventOwner = OwnerEnum.Enemy;

        private Agent _owner;
        private AgentRenderer _renderer;
        private Transform _cameraTransform;

        public void Initialize(ModuleOwner owner)
        {
            _owner = owner as Agent;
            _renderer = owner.GetModule<AgentRenderer>();
            Debug.Assert(stepShake != null, $"[FootstepShakeModule] stepShake 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(_renderer != null, $"[FootstepShakeModule] AgentRenderer 모듈 없음: {gameObject.name}", this);

            // 카메라는 Core 씬에 하나 있고 지역을 옮겨도 그대로라서 한 번만 찾는다
            _cameraTransform = Camera.main != null ? Camera.main.transform : null;

            if (_renderer != null)
                _renderer.OnFootstepTrigger += HandleFootstep;
        }

        private void OnDestroy()
        {
            if (_renderer != null)
                _renderer.OnFootstepTrigger -= HandleFootstep;
        }

        private void HandleFootstep()
        {
            if (stepShake == null || (_owner != null && _owner.IsDead))
                return;

            // 멀리 다른 방에서 걷는 발소리까지 화면을 흔들지 않게 거리를 본다
            if (_cameraTransform != null && (_cameraTransform.position - transform.position).sqrMagnitude > shakeRange * shakeRange)
                return;

            Bus<CameraShakeEvent>.Raise(shakeEventOwner, new CameraShakeEvent(stepShake));
        }
    }
}
