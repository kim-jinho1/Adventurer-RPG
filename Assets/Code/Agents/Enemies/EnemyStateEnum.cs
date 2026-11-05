using Unity.Behavior;

namespace Code.Agents.Enemies
{
    /// <summary>
    /// 적 상태를 나타내는 열거형입니다.
    /// </summary>
    [BlackboardEnum]
    public enum EnemyStateEnum
    {
        IDLE = 0,
        MOVE = 1,
        COMBAT = 2,
        HIT = 3,
        STUNNED = 4,
        DEAD = 5
    }
}
