using Code.Animations;

namespace Code.Agents.Characters.FSM
{
    /// <summary>
    /// 캐릭터가 전투 상태에서 벗어나는 상태를 나타내는 클래스입니다.
    /// </summary>
    public class CharacterCombatExitState : AbstractCombatToggleState
    {
        protected override bool IsDrawing => false;

        public CharacterCombatExitState(Agent agent, AnimParamSO paramSO) : base(agent, paramSO)
        { }
    }
}
