using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Code.Agents.Enemies.BT.Actions
{
    /// <summary>
    /// 적을 사망 처리하는 BT 액션입니다.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "DeadProcess",
        story: "Set [Enemy] to dead",
        category: "Action",
        id: "c370564ad3dc5bcd9aeac93e464d18da")]
    public partial class DeadProcessAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;

        protected override Status OnStart()
        {
            if(Enemy.Value == null)
                return Status.Failure;

            Enemy.Value.SetDead();
            return Status.Success;
        }
    }
}
