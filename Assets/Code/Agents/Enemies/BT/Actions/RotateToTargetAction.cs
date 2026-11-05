using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Code.Agents.Enemies.BT.Actions
{
    /// <summary>
    /// 적이 타깃을 향해 여러 프레임에 걸쳐 부드럽게 회전합니다. 거의 다 돌면 Success를 반환합니다.
    /// </summary>
    /// <summary>
    /// 적이 타깃 쪽으로 돌아서는 BT 액션입니다.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "RotateToTarget",
        story: "[Enemy] Rotate to [Target]",
        category: "Action",
        id: "414cf1cc1b9e63ab22f7e10a5dc3c7b3")]
    public partial class RotateToTargetAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<GameObject> Target;
        [SerializeReference] public BlackboardVariable<float> RotationSpeed = new(360f);   // 초당 회전 각도

        private const float _arriveAngle = 5f; // 이 각도 안으로 들어오면 다 돈 것으로 본다

        protected override Status OnStart()
        {
            if (Enemy.Value == null || Target.Value == null)
                return Status.Failure;

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Target.Value == null)
                return Status.Failure;

            Transform enemyTransform = Enemy.Value.transform;

            // 높이 차를 빼야 고블린이 앞뒤로 기울지 않는다
            Vector3 lookDirection = Target.Value.transform.position - enemyTransform.position;
            lookDirection.y = 0f;
            if (lookDirection.sqrMagnitude < 0.0001f)
                return Status.Success; // 위치가 겹치면 바라볼 방향이 없다

            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            enemyTransform.rotation = Quaternion.RotateTowards(
                enemyTransform.rotation, targetRotation, RotationSpeed.Value * Time.deltaTime);

            return Quaternion.Angle(enemyTransform.rotation, targetRotation) < _arriveAngle
                ? Status.Success
                : Status.Running;
        }
    }
}
