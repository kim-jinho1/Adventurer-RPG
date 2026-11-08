using Code.Agents.FSM;
using Code.Animations;
using Code.Combats;
using UnityEngine;

namespace Code.Agents.Characters.FSM
{
    /// <summary>
    /// 캐릭터 상태의 추상 클래스입니다.
    /// </summary>
    public abstract class AbstractCharacterState : AgentState
    {
        protected IMover _mover;
        protected Character _character;

        protected IWeaponModule _weaponModule;
        protected IAttackModule _attackModule;
        protected IAnimatorTrigger _trigger;
        private Transform _mainCameraTransform;

        protected const float _inputThreshold = 0.01f;     // sqrMagnitude와 비교하므로 입력 길이 0.1의 제곱

        public AbstractCharacterState(Agent agent, AnimParamSO paramSO) : base(agent, paramSO)
        {
            _character = agent as Character;
            _weaponModule = agent.GetModule<IWeaponModule>();
            _attackModule = agent.GetModule<IAttackModule>();
            _trigger = agent.GetModule<IAnimatorTrigger>();

            Debug.Assert(_character != null, $"[AbstractCharacterState] Character에 붙어 있지 않음: {agent.name}", agent);
            Debug.Assert(_weaponModule != null, $"[AbstractCharacterState] IWeaponModule 모듈 없음: {agent.name}", agent);
            Debug.Assert(_attackModule != null, $"[AbstractCharacterState] IAttackModule 모듈 없음: {agent.name}", agent);
            Debug.Assert(_trigger != null, $"[AbstractCharacterState] IAnimatorTrigger 모듈 없음: {agent.name}", agent);

            _mover = agent.GetModule<IMover>();
        }

        /// <summary>
        /// 무기를 들고 있으면 Locomotion_Combat, 아니면 이 상태의 기본 Locomotion을 재생합니다.
        /// </summary>
        protected void PlayLocomotion(bool isWeaponDrawn)
        {
            _renderer.CrossFadeState(isWeaponDrawn ? _character.LocomotionCombatParam.HashValue : _clipHash);
        }

        /// <summary>
        /// 맨손인데 전투 중이면 무기를 뽑는 상태로 바꿉니다. 바꿨으면 true를 반환합니다.
        /// </summary>
        protected bool TryEnterCombat()
        {
            if (_weaponModule.IsDrawn || !_attackModule.IsInBattle)
                return false;

            _character.ChangeState(CharacterStateEnum.COMBAT_ENTER);
            return true;
        }

        /// <summary>
        /// 입력을 카메라 기준 수평 월드 방향으로 바꿉니다. 메인 카메라가 없으면 Vector3.zero를 반환합니다.
        /// </summary>
        protected Vector3 GetCameraRelativeDirection(Vector2 input)
        {
            // 상태는 씬 로드 순서보다 먼저 만들어질 수 있어 카메라를 생성자가 아니라 처음 쓸 때 찾는다
            if (_mainCameraTransform == null && Camera.main != null)
                _mainCameraTransform = Camera.main.transform;

            if (_mainCameraTransform == null)
                return Vector3.zero;

            Vector3 forward = _mainCameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = _mainCameraTransform.right;
            right.y = 0f;
            right.Normalize();

            return (forward * input.y + right * input.x).normalized;
        }
    }
}
