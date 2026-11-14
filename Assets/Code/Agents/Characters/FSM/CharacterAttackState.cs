using Code.Agents.FSM;
using Code.Animations;

namespace Code.Agents.Characters.FSM
{
    /// <summary>
    /// 캐릭터 공격 상태를 나타내는 클래스입니다.
    /// </summary>
    public class CharacterAttackState : AbstractCharacterState, ICanDodgeState
    {
        private readonly CharacterAttackModule _characterAttackModule;

        public CharacterAttackState(Agent agent, AnimParamSO paramSO) : base(agent, paramSO)
        {
            _characterAttackModule = agent.GetModule<CharacterAttackModule>();
        }

        public override void Enter()
        {
            _mover.CanManualMovement = false;
            _characterAttackModule.OnAttackEnd += HandleAnimationEnd;
        }

        public override void HandleAnimationEnd()
        {
            _character.ChangeState(CharacterStateEnum.IDLE);
        }

        public override void Exit()
        {
            _mover.CanManualMovement = true;
            if (_characterAttackModule.CurrentAttack != null)
                _characterAttackModule.CurrentAttack.StopAttack();

            _characterAttackModule.OnAttackEnd -= HandleAnimationEnd;
            base.Exit();
        }
    }
}
