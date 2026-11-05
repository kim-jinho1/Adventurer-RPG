using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Code.Agents.Enemies.BT.Actions
{
    /// <summary>
    /// 감지 범위와 시야 안에서 타깃을 찾아 블랙보드에 넣는 BT 액션입니다.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "CheckTarget",
        story: "[Enemy] check [TargetGameObject] in range",
        category: "Action",
        id: "90435f3d6ab07703b98bf2b5e7017f03")]
    public partial class CheckTargetAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<GameObject> TargetGameObject;

        protected override Status OnStart()
        {
            if (Enemy.Value == null || Enemy.Value.Sensor == null)
                return Status.Failure;

            EnemyAttackConfigSO attackConfig = Enemy.Value.AttackConfig;
            AgentSensor sensor = Enemy.Value.Sensor;
            Vector3 startPosition = Enemy.Value.transform.position + new Vector3(0, 0.5f);

            if (sensor.IsTargetInRange(attackConfig.DetectRange, out Collider hitCollider)
                && sensor.IsTargetInSight(startPosition, attackConfig.DetectRange, hitCollider))
            {
                TargetGameObject.Value = hitCollider.gameObject;
                return Status.Success;
            }

            return Status.Failure;
        }

        protected override Status OnUpdate()
        {
            return Status.Success;
        }
    }
}
