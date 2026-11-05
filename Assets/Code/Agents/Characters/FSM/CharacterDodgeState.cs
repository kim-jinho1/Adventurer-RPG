using Code.Animations;
using UnityEngine;

namespace Code.Agents.Characters.FSM
{
    /// <summary>
    /// 캐릭터의 회피(구르기) 상태를 나타내는 클래스입니다. 앞부분 동안 무적이고, 클립이 끝나면 Idle로 돌아갑니다.
    /// </summary>
    public class CharacterDodgeState : AbstractCharacterState
    {
        // 애니메이터 ROLL 상태를 0.85배속으로 늦춰서 시간은 ÷0.85, 속도는 ×0.85로 맞춘다 (구르는 거리는 약 4.2m 그대로)
        private const float _invincibleDuration = 0.59f; // 시작 후 이 시간 동안 무적
        private const float _moveDuration = 0.82f;       // 시작 후 이 시간 동안 앞으로 밀린다
        private const float _dodgeSpeed = 5.1f;          // 구르는 속도 (m/s)

        private readonly Transform _mainCameraTransform;
        private IAnimatorTrigger _trigger;

        private float _invincibleEndTime;
        private float _moveEndTime;
        private bool _isMoving;

        public CharacterDodgeState(Agent agent, AnimParamSO paramSO) : base(agent, paramSO)
        {
            _trigger = agent.GetModule<IAnimatorTrigger>();
            Debug.Assert(_trigger != null, $"[CharacterDodgeState] IAnimatorTrigger 모듈 없음: {_character.name}", _character);

            if (Camera.main != null)
                _mainCameraTransform = Camera.main.transform;
        }

        public override void Enter()
        {
            base.Enter();
            _renderer.PlayClip(_clipHash, 0, 0f);

            Vector3 dodgeDirection = GetDodgeDirection();
            _character.transform.rotation = Quaternion.LookRotation(dodgeDirection);

            _mover.IsRotationLocked = true;
            _mover.StopImmediately();
            _mover.CanManualMovement = false;
            _mover.SetAutoMovement(dodgeDirection * _dodgeSpeed);

            _character.HealthModule.IsInvincible = true;
            _invincibleEndTime = Time.time + _invincibleDuration;
            _moveEndTime = Time.time + _moveDuration;
            _isMoving = true;

            _trigger.OnAnimationEnd += HandleAnimationEnd;
        }

        public override void Update()
        {
            base.Update();
            if (Time.time >= _invincibleEndTime && _character.HealthModule.IsInvincible)
                _character.HealthModule.IsInvincible = false;

            if (Time.time >= _moveEndTime && _isMoving)
            {
                _mover.StopImmediately();
                _isMoving = false;
            }
        }

        /// <summary>
        /// 입력 방향을 카메라 기준 월드 방향으로 바꿉니다. 입력이 없으면 캐릭터 정면으로 구릅니다.
        /// </summary>
        private Vector3 GetDodgeDirection()
        {
            Vector2 input = _character.CharacterInput.InputDirection;
            if (input.sqrMagnitude < _inputThreshold || _mainCameraTransform == null)
                return _character.transform.forward;

            Vector3 forward = _mainCameraTransform.forward;
            Vector3 right = _mainCameraTransform.right;
            forward.y = 0f;
            right.y = 0f;

            return (forward.normalized * input.y + right.normalized * input.x).normalized;
        }

        public override void HandleAnimationEnd()
        {
            _character.ChangeState(CharacterStateEnum.IDLE);
        }

        public override void Exit()
        {
            // 무적 시간 중에 다른 상태로 빠져도 무적이 남지 않게 반드시 끈다
            _character.HealthModule.IsInvincible = false;
            _mover.StopImmediately();
            _mover.CanManualMovement = true;
            _mover.IsRotationLocked = false;

            _trigger.OnAnimationEnd -= HandleAnimationEnd;
            base.Exit();
        }
    }
}
