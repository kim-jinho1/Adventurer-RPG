using Code.Animations;

namespace Code.Agents.Characters.FSM
{
    /// <summary>
    /// 캐릭터가 전투 상태로 진입하는 상태를 나타내는 클래스입니다.
    /// </summary>
    public class CharacterCombatEnterState : AbstractCombatToggleState
    {
        protected override bool IsDrawing => true;

        public CharacterCombatEnterState(Agent agent, AnimParamSO paramSO) : base(agent, paramSO)
        { }
    }
}
