using System;
using Code.Modules;
using UnityEngine;

namespace Code.Interactions
{
    /// <summary>
    /// 주변에서 가장 가까운 IInteractable을 찾고, 그 대상에 외곽선 · 말풍선을 켭니다. 작업 중에는 끕니다.
    /// 입력은 Character가 받고, 작업 정보는 WorkModule이 들고 있습니다.
    /// </summary>
    public class InteractionModule : MonoBehaviour, IModule, IInteractionModule
    {
        [Header("Interaction Setting")]
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private float interactRadius = 2f;
        [SerializeField, Min(0.02f)] private float focusRefreshInterval = 0.1f;

        /// <summary> 지금 외곽선 · 말풍선을 보여 주는 가장 가까운 대상입니다. 없으면 null입니다. </summary>
        public InteractableMarker FocusedMarker { get; private set; }

        public event Action<InteractableMarker> OnFocusChanged;

        private const int _maxOverlapCount = 8;

        private readonly Collider[] _overlapResults = new Collider[_maxOverlapCount];

        private ModuleOwner _owner;
        private IWorkModule _workModule;

        private float _nextFocusRefreshTime;

        public void Initialize(ModuleOwner owner)
        {
            _owner = owner;
            _workModule = _owner.GetModule<IWorkModule>();
            Debug.Assert(_workModule != null, $"[InteractionModule] IWorkModule 모듈 없음: {gameObject.name}", this);
        }

        private void Update()
        {
            if (_owner == null || Time.time < _nextFocusRefreshTime)
                return;

            _nextFocusRefreshTime = Time.time + focusRefreshInterval;

            // 채집 · 망치질 모션 위에 말풍선이 뜨면 거슬려서 작업 중에는 표시를 끈다
            IInteractable closest = !_workModule.IsWorking ? FindClosestInteractable() : null;
            InteractableMarker marker = closest is Component component ? component.GetComponent<InteractableMarker>() : null;
            SetFocusedMarker(marker);
        }

        /// <summary>
        /// 작업 없이 바로 상호작용합니다.
        /// </summary>
        public void InteractImmediately(IInteractable interactable)
        {
            interactable.Interact(_owner);
        }

        /// <summary>
        /// 주변에서 상호작용할 수 있는 가장 가까운 대상을 찾습니다. 없으면 null입니다.
        /// </summary>
        public IInteractable FindClosestInteractable()
        {
            Vector3 origin = _owner.transform.position;
            int count = Physics.OverlapSphereNonAlloc(origin, interactRadius, _overlapResults,
                interactableLayer, QueryTriggerInteraction.Collide);

            IInteractable closest = null;
            float closestSqrDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                // 콜라이더가 자식에 붙어 있어도 찾도록 부모까지 본다 (키를 눌렀을 때만 돌아 비용이 작음)
                IInteractable interactable = _overlapResults[i].GetComponentInParent<IInteractable>();
                if (interactable == null || !interactable.CanInteract(_owner))
                    continue;

                float sqrDistance = (_overlapResults[i].transform.position - origin).sqrMagnitude;
                if (sqrDistance >= closestSqrDistance)
                    continue;

                closestSqrDistance = sqrDistance;
                closest = interactable;
            }

            return closest;
        }

        private void SetFocusedMarker(InteractableMarker marker)
        {
            if (FocusedMarker == marker)
                return;

            if (FocusedMarker != null)
                FocusedMarker.SetFocused(false);

            FocusedMarker = marker;

            if (FocusedMarker != null)
                FocusedMarker.SetFocused(true);

            OnFocusChanged?.Invoke(FocusedMarker);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
