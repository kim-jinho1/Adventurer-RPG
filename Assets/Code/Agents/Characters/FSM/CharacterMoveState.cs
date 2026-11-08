using Code.Agents.FSM;
using Code.Animations;
using Code.Interactions;
using UnityEngine;

namespace Code.Agents.Characters.FSM
{
    /// <summary>
    /// 캐릭터의 Move 상태를 나타내는 클래스입니다.
    /// </summary>
    public class CharacterMoveState : AbstractCharacterState, ICanAttackState, ICanDodgeState, ICanWorkState, ICanToggleCombatState
    {
        public CharacterMoveState(Agent agent, AnimParamSO paramSO) : base(agent, paramSO)
        { }

        public override void Enter()
        {
            PlayLocomotion(_weaponModule.IsDrawn);
        }

        public override void Update()
        {
            base.Update();
            // 맨손인데 전투가 켜졌으면(경직 없는 공격에 맞음) 뽑는다
            if (TryEnterCombat())
                return;

            bool isSprinting = _character.CharacterInput.IsSprinting;
            Vector2 input = _character.CharacterInput.InputDirection;
            Vector3 movementDirection = GetCameraRelativeDirection(input);
            _mover.SetMoveSpeedMultiplier(isSprinting ? _character.SprintSpeedMultiplier : 1f);
            _mover.SetMovementDirection(movementDirection);
            // 이동 속도가 걷기 · 달리기 둘뿐이라 블렌드도 1(걷기) · 달리기 값으로 맞춘다 (기울기를 넣으면 대기 자세가 섞여 발이 미끄러짐)
            UpdateMoveBlend(movementDirection, isSprinting ? _character.SprintBlendSpeed : 1f);

            if (input.sqrMagnitude < _inputThreshold)
                _character.ChangeState(CharacterStateEnum.IDLE);
        }

        public override void Exit()
        {
            _mover.SetMoveSpeedMultiplier(1f);
            UpdateMoveBlend(Vector3.zero, 0f);
            base.Exit();
        }

        /// <summary>
        /// 월드 이동 방향을 캐릭터 로컬 기준으로 바꿔 2D 스트레이프 블렌드(MoveX/MoveY)에 넣습니다.
        /// 크기는 Speed 블렌드 값으로, 1 = Walk, SprintBlendSpeed = Run.
        /// </summary>
        private void UpdateMoveBlend(Vector3 worldDirection, float magnitude)
        {
            if (_character.MoveXParam == null || _character.MoveYParam == null || _character.SpeedParam == null)
                return;

            Vector3 local = _character.transform.InverseTransformDirection(worldDirection).normalized;

            _renderer.SetAnimFloat(_character.SpeedParam, magnitude, _character.MoveBlendDampTime);
            _renderer.SetAnimFloat(_character.MoveXParam, local.x, _character.MoveBlendDampTime);
            _renderer.SetAnimFloat(_character.MoveYParam, local.z, _character.MoveBlendDampTime);
        }
    }
}
