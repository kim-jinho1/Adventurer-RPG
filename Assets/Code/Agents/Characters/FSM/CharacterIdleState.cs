using Code.Agents.FSM;
using Code.Animations;
using Code.Combats;
using Code.Interactions;
using UnityEngine;

namespace Code.Agents.Characters.FSM
{
    /// <summary>
    /// 캐릭터의 Idle 상태를 나타내는 클래스입니다.
    /// </summary>
    public class CharacterIdleState : AbstractCharacterState, ICanAttackState, ICanDodgeState, ICanWorkState
    {
        private IWeaponModule _weaponModule;
        private IAttackModule _attackModule;

        public CharacterIdleState(Agent agent, AnimParamSO paramSO) : base(agent, paramSO)
        {
            _weaponModule = _character.GetModule<IWeaponModule>();
            _attackModule = _character.GetModule<IAttackModule>();
            Debug.Assert(_weaponModule != null, $"[CharacterIdleState] IWeaponModule 모듈 없음: {_character.name}", _character);
            Debug.Assert(_attackModule != null, $"[CharacterIdleState] IAttackModule 모듈 없음: {_character.name}", _character);
        }

        public override void Enter()
        {
            PlayLocomotion(_weaponModule.IsDrawn);
            _mover.StopImmediately();
        }

        public override void Update()
        {
            base.Update();
            // 맨손인데 전투가 켜졌으면(경직 없는 공격에 맞음) 뽑는다
            if (!_weaponModule.IsDrawn && _attackModule.IsInBattle)
            {
                _character.ChangeState(CharacterStateEnum.COMBAT_ENTER);
                return;
            }

            Vector2 input = _character.CharacterInput.InputDirection;
            _renderer.SetAnimFloat(_character.SpeedParam, 0f, _character.MoveBlendDampTime);
            if (input.sqrMagnitude > _inputThreshold)
                _character.ChangeState(CharacterStateEnum.MOVE);
        }
    }
}
