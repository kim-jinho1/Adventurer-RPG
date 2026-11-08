using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Code.Agents.Enemies.BT.Actions
{
    /// <summary>
    /// 적이 처음 자리(HomePosition)로 돌아가는 BT 액션입니다.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "ReturnHome",
        story: "[Enemy] Return Home",
        category: "Action",
        id: "ebd3e8f183e22ca90972740f12036eb8")]
    public partial class ReturnHomeAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;

        private IMover _mover;

        protected override Status OnStart()
        {
            if (Enemy.Value == null || Enemy.Value.Mover == null)
                return Status.Failure;

            _mover = Enemy.Value.Mover;
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            Vector3 homePosition = Enemy.Value.HomePosition;
            Vector3 currentPosition = Enemy.Value.transform.position;

            Vector3 direction = homePosition - currentPosition;
            direction.y = 0f;
            float arriveDistance = Enemy.Value.AttackConfig.HomeArriveDistance;
            if (direction.sqrMagnitude < arriveDistance * arriveDistance)
            {
                _mover.StopImmediately();
                return Status.Success;
            }

            _mover.SetMovementDirection((homePosition - currentPosition).normalized);
            return Status.Running;
        }

        protected override void OnEnd()
        {
            if (_mover == null)
                return;

            _mover.StopImmediately();
            base.OnEnd();
        }
    }
}
