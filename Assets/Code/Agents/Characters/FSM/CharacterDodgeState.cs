using Code.Animations;
using UnityEngine;

namespace Code.Agents.Characters.FSM
{
    /// <summary>
    /// 캐릭터의 회피(구르기) 상태를 나타내는 클래스입니다. 앞부분 동안 무적이고, 클립이 끝나면 Idle로 돌아갑니다.
    /// </summary>
    public class CharacterDodgeState : AbstractOneShotState
    {
        private float _invincibleEndTime;
        private float _moveEndTime;
        private bool _isMoving;

        public CharacterDodgeState(Agent agent, AnimParamSO paramSO) : base(agent, paramSO)
        { }

        public override void Enter()
        {
            base.Enter();
            _renderer.PlayClip(_clipHash, 0, 0f);

            Vector3 dodgeDirection = GetDodgeDirection();
            _character.transform.rotation = Quaternion.LookRotation(dodgeDirection);

            _mover.IsRotationLocked = true;
            _mover.StopImmediately();
            _mover.CanManualMovement = false;
            _mover.SetAutoMovement(dodgeDirection * _character.DodgeSpeed);

            _character.HealthModule.IsInvincible = true;
            _invincibleEndTime = Time.time + _character.DodgeInvincibleDuration;
            _moveEndTime = Time.time + _character.DodgeMoveDuration;
            _isMoving = true;
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
            if (input.sqrMagnitude < _inputThreshold)
                return _character.transform.forward;

            Vector3 direction = GetCameraRelativeDirection(input);
            return direction == Vector3.zero ? _character.transform.forward : direction;
        }

        public override void Exit()
        {
            // 무적 시간 중에 다른 상태로 빠져도 무적이 남지 않게 반드시 끈다
            _character.HealthModule.IsInvincible = false;
            _mover.StopImmediately();
            _mover.CanManualMovement = true;
            _mover.IsRotationLocked = false;
            base.Exit();
        }
    }
}
