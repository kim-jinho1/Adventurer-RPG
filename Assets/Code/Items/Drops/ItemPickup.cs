using System.Collections;
using Code.Inventories;
using Code.Modules;
using Code.Pools;
using UnityEngine;

namespace Code.Items.Drops
{
    /// <summary>
    /// 바닥에 떨어진 아이템입니다. 트리거 범위에 들어온 IItemCollector에 줍기를 요청하고, 받아 주면 그쪽으로 날아가 가방에 들어갑니다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ItemPickup : MonoBehaviour, IPickupable, IPoolable
    {
        [field: SerializeField] public PoolingItemSO PoolingType { get; private set; }

        public GameObject GameObject
            => gameObject;

        [field: Header("Item")]
        [field: SerializeField] public AbstractItemDataSO Item { get; private set; }

        [field: SerializeField, Min(1)] public int Count { get; private set; } = 1;

        [Header("Pickup Setting")]
        [SerializeField] private float pickupDelay = 0.6f;
        [SerializeField] private float magnetSpeed = 10f;
        [SerializeField] private float collectDistance = 0.3f;
        [SerializeField] private float partialDropDistance = 0.8f;
        [SerializeField] private SpriteRenderer iconRenderer;
        [SerializeField] private Collider bodyCollider;

        private Pool _myPool;                   // 풀 없이 만들어지면 null
        private Rigidbody _rigidbody;
        private IItemCollector _collector;
        private Object _collectorObject;

        private PickupStateEnum _state;

        private int _overlapCount;
        private WaitForSeconds _pickupWait;     // 풀링으로 다시 쓸 때 매번 새로 만들지 않게 한 번만 만들어 둔다

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (bodyCollider == null)
                bodyCollider = FindBodyCollider();

            RefreshIcon();
            _pickupWait = new WaitForSeconds(pickupDelay);

            // 풀 없이 만들어진 경우를 위해 여기서도 준비한다 (풀에서 만든 것은 바로 꺼져 코루틴이 멈추므로 꺼낼 때 다시 준비함)
            ResetItem();
        }

        private void OnDestroy()
        {
            // 한도를 넘어 부서지거나 풀 없는 씬에서 사라질 때 대기 줄에 파괴된 아이템이 남지 않게 뺀다
            CancelRequest();
        }

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        /// <summary>
        /// 꺼낼 때마다 튕기는 상태부터 다시 시작합니다. 아이템 · 개수는 뒤이어 Setup이 정합니다.
        /// </summary>
        public void ResetItem()
        {
            // 날던 채로 회수됐을 수 있어 키네마틱을 먼저 끈 뒤 속도를 비운다
            ChangeState(PickupStateEnum.Launching);
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            StartCoroutine(WaitPickupDelay());
        }

        /// <summary>
        /// 돌려놓기 전에 줍는 쪽과의 연결을 끊습니다.
        /// </summary>
        public void CleanUpItem()
        {
            CancelRequest();

            // 꺼질 때는 OnTriggerExit가 오지 않아 겹친 수가 남으므로 직접 비운다
            _overlapCount = 0;
            _collector = null;
            _collectorObject = null;
        }

        /// <summary>
        /// 떨어질 아이템과 개수를 정합니다. 생성 직후 호출합니다.
        /// </summary>
        public void Setup(AbstractItemDataSO item, int count)
        {
            Item = item;
            Count = count;
            RefreshIcon();
        }

        /// <summary>
        /// 생성 위치에서 튕겨 나가게 합니다.
        /// </summary>
        public void Launch(Vector3 impulse)
        {
            _rigidbody.AddForce(impulse, ForceMode.VelocityChange);
        }

        // 날아가는 동안에만 켜져 있다
        private void Update()
        {
            // 줍는 쪽이 파괴되면 인터페이스 참조만 남아 있으므로 UnityEngine.Object로 확인한다
            if (_collectorObject == null)
            {
                ReleaseCollector();
                return;
            }

            Vector3 target = _collector.CollectPoint.position;
            transform.position = Vector3.MoveTowards(transform.position, target, magnetSpeed * Time.deltaTime);
            if ((target - transform.position).sqrMagnitude <= collectDistance * collectDistance)
                CollectToTarget();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Item == null)
                return;

            IItemCollector collector = FindCollector(other);
            if (collector == null || (_collector != null && collector != _collector))
                return;

            // 줍는 쪽 콜라이더가 여러 개일 수 있어, 모두 나갔을 때만 놓아주도록 겹친 수를 센다
            _overlapCount++;
            if (_collector != null)
                return;

            _collector = collector;
            _collectorObject = collector as Object;

            // 튕기는 중이면 기억만 해 두고, 대기 시간이 끝날 때 요청한다
            if (_state == PickupStateEnum.Resting)
                SendRequest();
        }

        private void OnTriggerExit(Collider other)
        {
            // 날아가는 중에는 범위를 벗어나도 끝까지 따라간다
            if (_collector == null || FindCollector(other) != _collector)
                return;

            _overlapCount = Mathf.Max(_overlapCount - 1, 0);
            if (_state == PickupStateEnum.Flying || _overlapCount > 0)
                return;

            ReleaseCollector();
        }

        /// <summary>
        /// 줍는 쪽이 받아 주기로 했을 때 부릅니다. 줍는 쪽으로 날아가기 시작합니다.
        /// </summary>
        public void StartFlying()
        {
            ChangeState(PickupStateEnum.Flying);
        }

        // 튕겨 나가는 연출을 보여 주려고 잠시 기다린다
        private IEnumerator WaitPickupDelay()
        {
            yield return _pickupWait;

            ChangeState(PickupStateEnum.Resting);
            if (_collector != null)
                SendRequest();
        }

        // 줍는 쪽이 그 자리에서 StartFlying을 부를 수 있으므로 Requested를 먼저 넣는다
        private void SendRequest()
        {
            ChangeState(PickupStateEnum.Requested);
            _collector.RequestPickup(this);
        }

        private void CollectToTarget()
        {
            int leftover = _collector.Collect(Item, Count);
            if (leftover <= 0)
            {
                if (_myPool != null)
                    _myPool.Push(this);
                else
                    Destroy(gameObject);

                return;
            }

            // 일부만 들어갔으면 남은 개수로 앞쪽에 다시 놓고(몸 안에서 물리를 켜면 튕겨 나감) 줍는 쪽이 다시 받을지 정하도록 줄을 다시 선다
            Count = leftover;
            Transform collectPoint = _collector.CollectPoint;
            transform.position = collectPoint.position + collectPoint.forward * partialDropDistance;
            SendRequest();
        }

        private void ReleaseCollector()
        {
            CancelRequest();

            _overlapCount = 0;
            _collector = null;
            _collectorObject = null;

            // 튕기는 중에 지나간 것이면 대기 시간은 그대로 흐르게 둔다
            if (_state != PickupStateEnum.Launching)
                ChangeState(PickupStateEnum.Resting);
        }

        private void CancelRequest()
        {
            if (_state == PickupStateEnum.Requested && _collectorObject != null)
                _collector.CancelPickup(this);
        }

        // 날아가는 동안 몸통 콜라이더가 켜져 있으면 캐릭터 컨트롤러가 겹침을 풀려고 플레이어를 밀어내 영영 닿지 못한다
        private void ChangeState(PickupStateEnum nextState)
        {
            _state = nextState;
            // 꺼져 있어도 트리거 메시지와 코루틴은 계속 온다
            enabled = nextState == PickupStateEnum.Flying;

            bool isFlying = nextState == PickupStateEnum.Flying;
            _rigidbody.isKinematic = isFlying;
            if (bodyCollider != null)
                bodyCollider.enabled = !isFlying;
        }

        private Collider FindBodyCollider()
        {
            foreach (Collider candidate in GetComponents<Collider>())
                if (!candidate.isTrigger)
                    return candidate;

            Debug.LogWarning($"[ItemPickup] 트리거가 아닌 몸통 콜라이더 찾지 못함: {gameObject.name}", this);
            return null;
        }

        private void RefreshIcon()
        {
            if (iconRenderer != null && Item != null)
                iconRenderer.sprite = Item.Icon;
        }

        private static IItemCollector FindCollector(Collider other)
        {
            ModuleOwner owner = other.GetComponentInParent<ModuleOwner>();
            return owner != null ? owner.GetModule<IItemCollector>() : null;
        }
    }
}
