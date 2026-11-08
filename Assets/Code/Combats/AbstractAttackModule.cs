using System;
using System.Collections.Generic;
using Code.Modules;
using UnityEngine;

namespace Code.Combats
{
    /// <summary>
    /// 캐릭터·적 공격 모듈의 공통 부모입니다. 자식에 붙은 공격을 번호로 모으고, 전투 시간과 기본 데미지를 관리합니다.
    /// </summary>
    public abstract class AbstractAttackModule<TAttack> : MonoBehaviour, IModule, IAttackModule
        where TAttack : MonoBehaviour, IAttack
    {
        [field: SerializeField] public float BattleDuration { get; private set; } = 5f;

        public ModuleOwner Owner { get; private set; }

        public bool IsInBattle
            => Time.time - _lastAttackTime < BattleDuration;

        public event Action OnAttackEnd;

        protected Dictionary<int, TAttack> _attackDict;    // 공격 번호 → 공격
        protected float _currentDamage = 1f;

        private float _lastAttackTime = float.NegativeInfinity;

        public virtual void Initialize(ModuleOwner owner)
        {
            Owner = owner;
        }

        /// <summary>
        /// 자식에 붙은 공격을 번호로 모으고 초기화합니다. 자식 모듈이 Initialize 마지막에 부릅니다.
        /// </summary>
        protected void CollectAttacks()
        {
            _attackDict = new Dictionary<int, TAttack>();
            foreach (TAttack attack in GetComponentsInChildren<TAttack>())
                if (attack.AttackData == null || !_attackDict.TryAdd(attack.AttackData.Index, attack))
                    Debug.LogError($"[{GetType().Name}] AttackData 연결 안 됨 또는 Index 중복: {attack.gameObject.name}", this);

            foreach (TAttack attack in _attackDict.Values)
                InitializeAttack(attack);
        }

        /// <summary> 공격에 자기 자신을 넘겨 초기화합니다. 자식은 attack.InitializeAttack(this) 한 줄로 채웁니다. </summary>
        protected abstract void InitializeAttack(TAttack attack);

        public bool CanAttack(int attackIndex, GameObject target = null)
        {
            return _attackDict.TryGetValue(attackIndex, out TAttack targetAttack) &&
                   targetAttack.CanAttack(target);
        }

        public abstract void Attack(int attackIndex, GameObject target = null);

        public void StopAttack(int attackIndex)
        {
            if (_attackDict.TryGetValue(attackIndex, out TAttack targetAttack))
                targetAttack.StopAttack();
        }

        public float GetBaseDamage(AttackDataSO attackData)
        {
            return _currentDamage * attackData.DamageMultiplier;
        }

        public void SetLastBattle()
            => _lastAttackTime = Time.time;

        public void ClearLastBattle()
            => _lastAttackTime = float.NegativeInfinity;

        /// <summary> 공격이 끝났음을 알립니다(OnAttackEnd). </summary>
        public virtual void InvokeAttackEnd()
            => OnAttackEnd?.Invoke();
    }
}
