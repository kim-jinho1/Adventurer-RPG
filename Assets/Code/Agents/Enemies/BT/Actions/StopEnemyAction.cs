using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Code.Agents.Enemies.BT.Actions
{
    /// <summary>
    /// 적의 이동을 멈추는 BT 액션입니다.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "StopEnemy",
        story: "[Enemy] Stop",
        category: "Action",
        id: "72481824bfa926a4580b18bc7ba714e3")]
    public partial class StopEnemyAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;

        protected override Status OnStart()
        {
            if(Enemy.Value == null || Enemy.Value.Mover == null)
                return Status.Failure;

            Enemy.Value.Mover.StopImmediately();
            return Status.Success;
        }
    }
}
