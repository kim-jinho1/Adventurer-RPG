namespace Code.Agents.Characters.FSM
{
    /// <summary>
    /// 캐릭터 상태를 나타내는 열거형입니다.
    /// </summary>
    public enum CharacterStateEnum
    {
        IDLE = 0,
        MOVE = 1,
        ATTACK = 2,
        HIT = 3,
        DEAD = 4,
        DODGE = 5,
        COMBAT_ENTER = 6,
        COMBAT_EXIT = 7,
        WORK = 8,
    }
}
