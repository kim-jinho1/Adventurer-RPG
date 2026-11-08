using UnityEngine;

namespace Code.Worlds
{
    /// <summary>
    /// 지역 씬 안에서 플레이어가 도착하는 위치입니다. 이 오브젝트의 위치와 앞 방향(파란 축)으로 플레이어를 옮깁니다.
    /// 같은 씬 안에서는 Id가 겹치지 않아야 합니다.
    /// </summary>
    public class AreaSpawnPoint : MonoBehaviour
    {
        [field: Header("Spawn Point Setting")]
        [field: SerializeField] public SpawnPointSO Id { get; private set; }

        private void Awake()
        {
            Debug.Assert(Id != null, $"[AreaSpawnPoint] Id 연결 안 됨: {gameObject.name}", this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.5f);
            Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward);
        }
    }
}
