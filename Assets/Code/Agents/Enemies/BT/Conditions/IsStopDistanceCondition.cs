using System;
using Unity.Behavior;
using UnityEngine;

namespace Code.Agents.Enemies.BT.Conditions
{
    /// <summary>
    /// 타깃이 멈춤 거리(StoppingDistance) 안에 있는지, 또는 밖에 있는지 보는 BT 조건입니다.
    /// </summary>
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(
        name: "IsStopDistance",
        story: "[Enemy] distance from [Target] is In: [IsIn]",
        category: "Conditions",
        id: "05b1b3418560d4a617b90047a84e739b")]
    public partial class IsStopDistanceCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<GameObject> Target;
        [SerializeReference] public BlackboardVariable<bool> IsIn = new(true);

        public override bool IsTrue()
        {
            if (Enemy.Value == null || Enemy.Value.AttackConfig == null || Target.Value == null)
                return false;

            // ChaseTargetAction과 같은 XZ 거리 기준을 써야 추적 ↔ 정지 판정이 엇갈리지 않는다
            float sqrDistance = Enemy.Value.GetFlatSqrDistance(Target.Value.transform.position);
            float stoppingDistance = Enemy.Value.AttackConfig.StoppingDistance;
            return IsIn.Value
                ? sqrDistance < stoppingDistance * stoppingDistance
                : sqrDistance >= stoppingDistance * stoppingDistance;
        }
    }
}
