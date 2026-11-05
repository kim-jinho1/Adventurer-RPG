using Code.CoreSystem.Bus;
using Code.GameEvents.Worlds;
using UnityEngine;

namespace Code.Pools
{
    /// <summary>
    /// Core 씬에 두는 풀 관리자입니다. 시작할 때 풀을 만들고, 지역을 떠나기 전에 꺼낸 것을 모두 회수합니다.
    /// </summary>
    public class PoolManagerMono : MonoBehaviour
    {
        [SerializeField] private PoolManagerSO poolManager;

        [Header("Event")]
        [SerializeField] private OwnerEnum eventOwner = OwnerEnum.Player;

        private BusSubscriptionScope _busScope;

        private void Awake()
        {
            poolManager.Initialize(transform);

            _busScope = new BusSubscriptionScope();
            _busScope.Subscribe<AreaLeavingEvent>(eventOwner, HandleAreaLeaving);
        }

        private void OnDestroy()
        {
            _busScope?.Dispose();
            poolManager.Clear();
        }

        // 꺼낸 오브젝트는 Core 씬에 있어서 지역 씬을 내려도 남으므로 먼저 회수한다
        private void HandleAreaLeaving(AreaLeavingEvent evt)
        {
            poolManager.PushAll();
        }
    }
}
