using Code.Animations;

namespace Code.Agents.Characters.FSM
{
    /// <summary>
    /// 클립을 한 번 재생하고 끝나면 Idle로 돌아가는 상태의 추상 클래스입니다.
    /// </summary>
    public abstract class AbstractOneShotState : AbstractCharacterState
    {
        protected AbstractOneShotState(Agent agent, AnimParamSO paramSO) : base(agent, paramSO)
        { }

        public override void Enter()
        {
            base.Enter();
            _trigger.OnAnimationEnd += HandleAnimationEnd;
        }

        public override void Exit()
        {
            _trigger.OnAnimationEnd -= HandleAnimationEnd;
            base.Exit();
        }

        public override void HandleAnimationEnd()
        {
            _character.ChangeState(CharacterStateEnum.IDLE);
        }
    }
}
