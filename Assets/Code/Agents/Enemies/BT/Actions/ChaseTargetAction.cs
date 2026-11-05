using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Code.Agents.Enemies.BT.Actions
{
    /// <summary>
    /// 적이 타깃을 향해 이동하는 BT 액션입니다.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "ChaseTarget",
        story: "[Enemy] going to [Target]",
        category: "Action",
        id: "548b26a9cf29a7bd73f83ae27b71616c")]
    public partial class ChaseTargetAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<GameObject> Target;

        private IMover _mover;
        private EnemyAttackConfigSO _attackConfig;

        protected override Status OnStart()
        {
            if (Enemy.Value == null || Target.Value == null)
                return Status.Failure;

            _mover = Enemy.Value.Mover;
            _attackConfig = Enemy.Value.AttackConfig;
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Target.Value == null)
                return Status.Failure;

            Vector3 direction = Target.Value.transform.position - Enemy.Value.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < _attackConfig.StoppingDistance * _attackConfig.StoppingDistance)
            {
                _mover.StopImmediately();
                return Status.Success;
            }

            _mover.SetMovementDirection(direction.normalized);
            return Status.Running;
        }

        protected override void OnEnd()
        {
            if (_mover == null)
                return;

            _mover.StopImmediately();
        }
    }
}
