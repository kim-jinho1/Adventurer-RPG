using System;
using Code.Agents.Stats;
using Code.Combats;
using Code.Modules;
using UnityEngine;

namespace Code.Agents
{
    /// <summary>
    /// 에이전트의 현재 체력을 관리하는 모듈입니다. 최대 체력은 스탯을 구독해서 따라갑니다.
    /// </summary>
    public class HealthModule : MonoBehaviour, IModule, IAfterInitModule, IHealthModule, IDamageable
    {
        [field: Header("Health Setting")]
        [field: SerializeField] public float MaxHealth { get; private set; } = 30f;
        [SerializeField] private StatSO maxHealthStat;

        public event IHealthModule.HealthChange OnHealthChange;
        public event Action OnDamaged;

        public float CurrentHealth
        {
            get => _currentHealth;
            private set
            {
                float before = _currentHealth;
                _currentHealth = Mathf.Clamp(value, 0f, MaxHealth);
                if (!Mathf.Approximately(before, _currentHealth))
                    OnHealthChange?.Invoke(before, _currentHealth, MaxHealth);
            }
        }

        public bool IsInvincible { get; set; } = false;

        private Agent _owner;

        private ActionDataModule _actionDataModule;
        private IStatModule _statModule;

        private float _currentHealth;

        public void Initialize(ModuleOwner owner)
        {
            _owner = owner as Agent;
            _statModule = owner.GetModule<IStatModule>();
            _actionDataModule = owner.GetModule<ActionDataModule>();
        }

        public void AfterInitialize()
        {
            Debug.Assert(maxHealthStat != null, $"[HealthModule] maxHealthStat 연결 안 됨: {gameObject.name}", this);
            if (_statModule != null)
                MaxHealth = _statModule.SubscribeStat(maxHealthStat.AssetIndex, HandleMaxHealthStatChange, MaxHealth);

            CurrentHealth = MaxHealth;
        }

        private void OnDestroy()
        {
            if (_statModule != null)
                _statModule.UnSubscribeStat(maxHealthStat.AssetIndex, HandleMaxHealthStatChange);
        }

        /// <summary>
        /// 피격 정보를 기록하고 데미지를 적용합니다.
        /// </summary>
        public void ApplyDamage(DamageData damageData, Vector3 hitPoint, Vector3 hitDirection, Vector3 hitNormal)
        {
            if(_owner.IsDead || IsInvincible)
                return;

            // 피격 반응(넉백·이펙트·데미지 숫자)이 읽어 갈 수 있도록 먼저 기록한다
            _actionDataModule.LastHitDirection = hitDirection;
            _actionDataModule.LastHitPoint = hitPoint;
            _actionDataModule.LastHitNormal = hitNormal;
            _actionDataModule.LastKnockbackForce = damageData.KnockBackForce;
            _actionDataModule.LastDamage = damageData.DamageAmount;
            _actionDataModule.IsLastHitCritical = damageData.IsCritical;
            _actionDataModule.IsLastHitStagger = damageData.CausesStagger;

            // 죽는 한 방이면 이 안에서 HandleHealthChange가 IsDead를 켠다
            CurrentHealth -= damageData.DamageAmount;
            OnDamaged?.Invoke(); // 피격 이벤트
            // 사망과 피격이 겹치지 않게 살아 있을 때만 알리고, 강약 구분은 받는 쪽이 IsLastHitStagger로 한다
            if (!_owner.IsDead)
                _owner.OnHit?.Invoke();
        }

        /// <summary>
        /// 받은 치유량을 적용합니다.
        /// </summary>
        public void Heal(float health)
            => CurrentHealth += health;

        /// <summary>
        /// 최대 체력 변화를 업데이트 합니다. 현재 체력은 비율을 유지합니다.
        /// </summary>
        private void HandleMaxHealthStatChange(StatSO stat, float currentValue, float prevValue)
        {
            // 차이만큼 더하면 장비를 벗었다 입을 때 체력이 복사되고, 빼면 벗다가 죽을 수 있다
            float changeMaxHealth = MaxHealth > 0f ? _currentHealth / MaxHealth : 0f;
            MaxHealth = currentValue;
            CurrentHealth = changeMaxHealth * MaxHealth;
        }
    }
}
