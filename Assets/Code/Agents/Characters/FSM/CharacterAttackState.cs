using Code.Agents.FSM;
using Code.Animations;

namespace Code.Agents.Characters.FSM
{
    /// <summary>
    /// 캐릭터 공격 상태를 나타내는 클래스입니다.
    /// </summary>
    public class CharacterAttackState : AbstractCharacterState, ICanDodgeState
    {
        private readonly CharacterAttackModule _attackModule;

        public CharacterAttackState(Agent agent, AnimParamSO paramSO) : base(agent, paramSO)
        {
            _attackModule = agent.GetModule<CharacterAttackModule>();
        }

        public override void Enter()
        {
            _isTriggerCall = false;
            _mover.CanManualMovement = false;
            _attackModule.OnAttackEnd += HandleAnimationEnd;
        }

        public override void Update()
        {
            base.Update();
            if (_isTriggerCall)
                _character.ChangeState(CharacterStateEnum.IDLE);
        }

        public override void Exit()
        {
            _mover.CanManualMovement = true;
            if (_attackModule.CurrentAttack != null)
                _attackModule.CurrentAttack.StopAttack();

            _attackModule.OnAttackEnd -= HandleAnimationEnd;
            base.Exit();
        }
    }
}
