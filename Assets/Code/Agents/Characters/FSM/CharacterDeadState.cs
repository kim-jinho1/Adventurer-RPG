using Code.Animations;

namespace Code.Agents.Characters.FSM
{
    /// <summary>
    /// 캐릭터의 Dead 상태를 나타내는 클래스입니다.
    /// </summary>
    public class CharacterDeadState : AbstractCharacterState
    {
        public CharacterDeadState(Agent agent, AnimParamSO paramSO) : base(agent, paramSO)
        { }

        public override void Enter()
        {
            base.Enter();
            _mover.StopImmediately();
            _renderer.PlayClip(_clipHash, 0, 0f);
        }
    }
}
