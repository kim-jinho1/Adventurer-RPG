namespace Code.Agents.FSM
{
    /// <summary>
    /// 공격 입력을 받을 수 있는 상태입니다. 조건부로 막아야 하는 상태는 CanAttack을 덮어씁니다.
    /// </summary>
    public interface ICanAttackState
    {
        bool CanAttack
            => true;
    }
}
