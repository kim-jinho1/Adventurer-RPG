using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Code.Agents.Enemies.BT.Actions
{
    /// <summary>
    /// 타깃이 놓치는 거리(LoseRange) 밖으로 나갈 때까지 기다리는 BT 액션입니다.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "WaitForTargetLost",
        story: "[Enemy] is Lost [Target]",
        category: "Action",
        id: "09b9040531e68ab5fb8e80d9bbc53dda")]
    public partial class WaitForTargetLostAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<GameObject> Target;

        protected override Status OnStart()
        {
            if(Enemy.Value == null || Target.Value == null || Enemy.Value.AttackConfig == null)
                return Status.Success;

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if(Target.Value == null)
                return Status.Success;

            return Enemy.Value.GetFlatSqrDistance(Target.Value.transform.position) >
                   Enemy.Value.AttackConfig.LoseRange * Enemy.Value.AttackConfig.LoseRange
                ? Status.Success
                : Status.Running;
        }
    }
}
