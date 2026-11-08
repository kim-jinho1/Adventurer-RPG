using Code.CoreSystem.Bus;
using Code.GameEvents.Worlds;
using UnityEngine;

namespace Code.Worlds
{
    /// <summary>
    /// 플레이어가 닿으면 다른 지역으로 옮겨 달라고 요청하는 트리거입니다. 트리거 콜라이더와 함께 둡니다.
    /// 도착 지점은 이 트리거 바깥에 둬야 도착하자마자 되돌아가지 않습니다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class AreaPortal : MonoBehaviour
    {
        [field: Header("Portal Setting")]
        [field: SerializeField] public AreaSO TargetArea { get; private set; }
        [field: SerializeField] public SpawnPointSO TargetSpawnPoint { get; private set; }

        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private OwnerEnum eventOwner = OwnerEnum.Player;

        private void Awake()
        {
            Debug.Assert(TargetArea != null, $"[AreaPortal] TargetArea 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(TargetSpawnPoint != null, $"[AreaPortal] TargetSpawnPoint 연결 안 됨: {gameObject.name}", this);
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((playerLayer.value & (1 << other.gameObject.layer)) == 0)
                return;

            Bus<AreaTransitionRequestEvent>.Raise(eventOwner, new AreaTransitionRequestEvent(TargetArea, TargetSpawnPoint));
        }
    }
}
