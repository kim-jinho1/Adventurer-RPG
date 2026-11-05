using Code.Agents;
using Code.CoreSystem.Bus;
using Code.GameEvents.Feedbacks;
using Code.Modules;
using Code.Pools;
using UnityEngine;

namespace Code.Combats.Feedbacks
{
    /// <summary>
    /// 맞을 때 맞은 위치에 이펙트를 띄우고 카메라를 흔듭니다. 경직 공격과 막타는 강하게, 나머지는 약하게 반응합니다.
    /// </summary>
    public class HitFeedbackModule : MonoBehaviour, IModule, IAfterInitModule
    {
        [Header("Effect")]
        [SerializeField] private PoolManagerSO poolManager;
        [SerializeField] private PoolingItemSO weakHitEffect;
        [SerializeField] private PoolingItemSO strongHitEffect;
        [SerializeField, Min(0f)] private float effectLifetime = 2f;

        [Header("Camera Shake")]
        [SerializeField] private ShakeDataSO weakHitShake;
        [SerializeField] private ShakeDataSO strongHitShake;
        [SerializeField] private OwnerEnum shakeEventOwner = OwnerEnum.Player;

        private IHealthModule _healthModule;
        private ActionDataModule _actionDataModule;

        public void Initialize(ModuleOwner owner)
        {
            _healthModule = owner.GetModule<IHealthModule>();
            _actionDataModule = owner.GetModule<ActionDataModule>();
            Debug.Assert(_healthModule != null, $"[HitFeedbackModule] IHealthModule 모듈 없음: {gameObject.name}", this);
            Debug.Assert(_actionDataModule != null, $"[HitFeedbackModule] ActionDataModule 모듈 없음: {gameObject.name}", this);
        }

        public void AfterInitialize()
        {
            if (_healthModule != null)
                _healthModule.OnDamaged += HandleDamaged;
        }

        private void OnDestroy()
        {
            if (_healthModule != null)
                _healthModule.OnDamaged -= HandleDamaged;
        }

        private void HandleDamaged()
        {
            // 막타는 경직이 없어도 강하게 — 쓰러뜨리는 손맛
            bool isStrong = _actionDataModule.IsLastHitStagger || _healthModule.CurrentHealth <= 0f;

            Vector3 normal = _actionDataModule.LastHitNormal;
            Quaternion rotation = normal.sqrMagnitude > 0f ? Quaternion.LookRotation(normal) : Quaternion.identity;
            FeedbackEffect.Spawn(poolManager, isStrong ? strongHitEffect : weakHitEffect,
                _actionDataModule.LastHitPoint, rotation, effectLifetime);

            ShakeDataSO shake = isStrong ? strongHitShake : weakHitShake;
            if (shake != null)
                Bus<CameraShakeEvent>.Raise(shakeEventOwner, new CameraShakeEvent(shake));
        }
    }
}
