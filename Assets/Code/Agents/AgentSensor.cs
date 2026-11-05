using Code.Modules;
using UnityEngine;

namespace Code.Agents
{
    /// <summary>
    /// AgentSensor 클래스는 Agent의 주변 환경을 감지하는 기능을 제공합니다.
    /// </summary>
    public class AgentSensor : MonoBehaviour, IModule
    {
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private LayerMask targetLayer;

        [SerializeField] private Vector3 boxSize;
        [SerializeField] private Vector3 offset;

        private readonly Collider[] _overlapResults = new Collider[8];

        private ModuleOwner _owner;

        public void Initialize(ModuleOwner owner)
        {
            _owner = owner;
        }

        /// <summary>
        /// 주어진 방향으로 장애물이 있는지 확인합니다.
        /// </summary>
        /// <param name="direction"> 확인할 방향 </param>
        /// <param name="hitCollider"> 장애물의 Collider </param>
        /// <returns> 장애물이 있는지 여부 </returns>
        public bool IsObstaclePresent(Vector3 direction, out Collider hitCollider)
        {
            Vector3 position = transform.position + direction + offset;
            int count = Physics.OverlapBoxNonAlloc(position, boxSize * 0.5f, _overlapResults,
                transform.rotation, obstacleLayer);

            hitCollider = count > 0 ? _overlapResults[0] : null;
            return hitCollider != null;
        }

        /// <summary>
        /// 주어진 방향으로 BoxCast를 수행하여 장애물과의 충돌 여부를 확인합니다.
        /// </summary>
        /// <param name="direction"> 확인할 방향 </param>
        /// <param name="distance"> 확인할 거리 </param>
        /// <param name="hit"> 충돌 정보 </param>
        /// <returns> 장애물과의 거리 </returns>
        public float BoxCastObstacle(Vector3 direction, float distance, out RaycastHit hit)
        {
            bool isHit = Physics.BoxCast(transform.position + offset, boxSize * 0.5f, direction,
                out hit, transform.rotation, distance, obstacleLayer);

            distance = isHit ? hit.distance : distance;
            return distance;
        }

        /// <summary>
        /// 주어진 범위 내에 타겟이 있는지 확인합니다.
        /// </summary>
        /// <param name="range"> 확인할 범위 </param>
        /// <param name="hitCollider"> 타겟의 Collider </param>
        /// <returns> 타겟이 있는지 여부 </returns>
        public bool IsTargetInRange(float range, out Collider hitCollider)
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, range, _overlapResults, targetLayer);
            hitCollider = count > 0 ? _overlapResults[0] : null;
            return hitCollider != null;
        }

        /// <summary>
        /// 주어진 시작 위치에서 타겟까지의 시야가 확보되어 있는지 확인합니다.
        /// </summary>
        /// <param name="startPosition"> 시작 위치 </param>
        /// <param name="range"> 확인할 범위 </param>
        /// <param name="target"> 타겟의 Collider </param>
        /// <returns> 시야가 확보되어 있는지 여부 </returns>
        public bool IsTargetInSight(Vector3 startPosition, float range, Collider target)
        {
            Vector3 direction = target.transform.position - startPosition;
            bool blocked = Physics.Raycast(startPosition, direction.normalized, direction.magnitude, obstacleLayer);
            return !blocked;
        }

        /// <summary> 선택된 게임 오브젝트를 그립니다. </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube( transform.position + offset, boxSize);
        }
    }
}
