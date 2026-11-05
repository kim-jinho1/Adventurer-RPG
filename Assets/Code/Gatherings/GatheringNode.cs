using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using Code.Combats.Feedbacks;
using Code.CoreSystem.Bus;
using Code.GameEvents.Feedbacks;
using Code.Interactions;
using Code.Inventories;
using Code.Items;
using Code.Items.Drops;
using Code.Levels;
using Code.Modules;
using Code.Pools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Code.Gatherings
{
    /// <summary>
    /// 상호작용하면 드랍 테이블대로 아이템을 주고 사라졌다가, 일정 시간 뒤 다시 생기는 채집 지점입니다. (철광석, 목재, 약초 등)
    /// </summary>
    public class GatheringNode : MonoBehaviour, IWorkInteractable, IWorkHitReceiver
    {
        [field: Header("Gathering Node Setting")]
        [field: SerializeField] public WorkTypeSO WorkType { get; private set; }

        [Header("Gathering Setting")]
        [SerializeField] private DropTableSO yieldTable;
        [SerializeField, Min(0f)] private float respawnTime = 30f;
        [SerializeField] private OwnerEnum expEventOwner = OwnerEnum.Player;
        [SerializeField] private GameObject visual;              // 자식이어야 한다 (자기 자신을 끄면 재생성 코루틴도 멈춤)
        [SerializeField] private ItemScatterSO overflowScatter;

        [Header("Work Hit Feedback")]
        [SerializeField] private PoolManagerSO poolManager;
        [SerializeField] private PoolingItemSO workHitEffect;
        [SerializeField] private Transform workHitPoint;           // 비우면 노드 위치 + 1m
        [SerializeField] private ShakeDataSO workHitShake;
        [SerializeField] private OwnerEnum shakeEventOwner = OwnerEnum.Player;
        [SerializeField, Min(0f)] private float effectLifetime = 2f;
        [SerializeField] private SoundID workHitSound;              // 도끼 · 곡괭이 · 손이 닿는 순간 (나무 · 돌 · 풀)

        [field: FormerlySerializedAs("OnGathered")]
        [field: SerializeField] public UnityEvent OnGathered { get; private set; }
        [field: FormerlySerializedAs("OnRespawned")]
        [field: SerializeField] public UnityEvent OnRespawned { get; private set; }

        public bool IsDepleted { get; private set; }    // 다 캐서 재생성을 기다리는 중

        private readonly List<ItemAmount> _yieldResults = new();

        private WaitForSeconds _respawnWait;

        private float _respawnAt;

        private void Awake()
        {
            Debug.Assert(WorkType != null, $"[GatheringNode] WorkType 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(yieldTable != null, $"[GatheringNode] yieldTable 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(visual != gameObject, $"[GatheringNode] visual은 자식이어야 함 (자신을 끄면 재생성이 멈춤): {gameObject.name}", this);
            Debug.Assert(overflowScatter != null, $"[GatheringNode] overflowScatter 연결 안 됨: {gameObject.name}", this);

            _respawnWait = new WaitForSeconds(respawnTime);
        }

        private void OnEnable()
        {
            // 기다리는 중에 오브젝트가 꺼지면 코루틴이 멈추므로, 다시 켜질 때 남은 시간부터 이어 간다
            if (IsDepleted)
            {
                float remainTime = _respawnAt - Time.time;
                StartCoroutine(remainTime > 0f ?
                    RespawnRoutine(new WaitForSeconds(remainTime)) : RespawnRoutine(null));
            }
        }

        public bool CanInteract(ModuleOwner interactor)
            => !IsDepleted && yieldTable != null && interactor != null && interactor.GetModule<IItemCollector>() != null;

        public void Interact(ModuleOwner interactor)
        {
            if (!CanInteract(interactor))
                return;

            IItemCollector collector = interactor.GetModule<IItemCollector>();
            yieldTable.Roll(_yieldResults);

            // 가방에 못 넣은 만큼은 바닥에 떨어뜨려 노드가 사라져도 보상이 남게 한다
            foreach (ItemAmount result in _yieldResults)
            {
                int leftover = collector.Collect(result.Item, result.Count);
                if (leftover > 0 && overflowScatter != null)
                    overflowScatter.Spawn(transform.position, result.Item, leftover);
            }

            // 적 드랍(DropModule)과 같은 경험치 버스로 보내, 경험치를 받는 길을 하나로 둔다
            if (yieldTable.Exp > 0)
                Bus<ExpGainEvent>.Raise(expEventOwner, new ExpGainEvent(yieldTable.Exp));

            Deplete();
        }

        private void Deplete()
        {
            IsDepleted = true;
            _respawnAt = Time.time + respawnTime;
            SetVisualActive(false);
            OnGathered?.Invoke();
            StartCoroutine(RespawnRoutine(_respawnWait));
        }

        private IEnumerator RespawnRoutine(WaitForSeconds wait)
        {
            yield return wait;
            IsDepleted = false;
            SetVisualActive(true);
            OnRespawned?.Invoke();
        }

        private void SetVisualActive(bool isActive)
        {
            if (visual != null)
                visual.SetActive(isActive);
        }

        public void HandleWorkHit(ModuleOwner worker)
        {
            if (IsDepleted)
                return;

            Vector3 point = workHitPoint != null ? workHitPoint.position : transform.position + Vector3.up;
            FeedbackEffect.Spawn(poolManager, workHitEffect, point, Quaternion.identity, effectLifetime);
            if (workHitSound.IsValid())
                BroAudio.Play(workHitSound, point);

            if (workHitShake != null)
                Bus<CameraShakeEvent>.Raise(shakeEventOwner, new CameraShakeEvent(workHitShake));
        }
    }
}
