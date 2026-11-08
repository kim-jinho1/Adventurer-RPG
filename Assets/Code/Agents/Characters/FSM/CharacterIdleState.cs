using Code.Agents.FSM;
using Code.Animations;
using Code.Interactions;
using UnityEngine;

namespace Code.Agents.Characters.FSM
{
    /// <summary>
    /// 캐릭터의 Idle 상태를 나타내는 클래스입니다.
    /// </summary>
    public class CharacterIdleState : AbstractCharacterState, ICanAttackState, ICanDodgeState, ICanWorkState, ICanToggleCombatState
    {
        public CharacterIdleState(Agent agent, AnimParamSO paramSO) : base(agent, paramSO)
        { }

        public override void Enter()
        {
            PlayLocomotion(_weaponModule.IsDrawn);
            _mover.StopImmediately();
        }

        public override void Update()
        {
            base.Update();
            // 맨손인데 전투가 켜졌으면(경직 없는 공격에 맞음) 뽑는다
            if (TryEnterCombat())
                return;

            Vector2 input = _character.CharacterInput.InputDirection;
            _renderer.SetAnimFloat(_character.SpeedParam, 0f, _character.MoveBlendDampTime);
            if (input.sqrMagnitude > _inputThreshold)
                _character.ChangeState(CharacterStateEnum.MOVE);
        }
    }
}
