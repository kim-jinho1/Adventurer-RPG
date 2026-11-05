using System;
using System.Collections.Generic;
using Code.Combats;
using Code.Modules;
using UnityEngine;

namespace Code.Agents.Enemies
{
    /// <summary>
    /// 적 공격 모듈입니다. 자식에 붙은 공격을 번호로 모아 시전하고 전투 상태를 관리합니다.
    /// </summary>
    public class EnemyAttackModule : MonoBehaviour, IModule, IAttackModule
    {
        [field: SerializeField] public float BattleDuration { get; private set; } = 5f;

        public ModuleOwner Owner { get; private set; }
        public AbstractEnemy OwnerEnemy { get; private set; }

        public bool IsInBattle
            => Time.time - _lastAttackTime < BattleDuration;

        public event Action OnAttackEnd;

        private Dictionary<int, AbstractEnemyAttack> _attackDict;

        private float _lastAttackTime = float.NegativeInfinity;
        private float _currentDamage = 1f;

        public void Initialize(ModuleOwner owner)
        {
            Owner = owner;
            OwnerEnemy = owner as AbstractEnemy;

            Debug.Assert(OwnerEnemy != null, $"[EnemyAttackModule] AbstractEnemy에 붙어 있지 않음: {gameObject.name}", this);

            _attackDict = new Dictionary<int, AbstractEnemyAttack>();
            foreach (AbstractEnemyAttack enemyAttack in GetComponentsInChildren<AbstractEnemyAttack>())
                if (enemyAttack.AttackData == null || !_attackDict.TryAdd(enemyAttack.AttackData.Index, enemyAttack))
                    Debug.LogError($"[EnemyAttackModule] AttackData 연결 안 됨 또는 Index 중복: {enemyAttack.gameObject.name}", this);

            foreach (AbstractEnemyAttack attack in _attackDict.Values)
                attack.InitializeAttack(this);

            _currentDamage = OwnerEnemy.AttackConfig.Damage;
        }

        public bool CanAttack(int attackIndex, GameObject target = null)
        {
            return _attackDict.TryGetValue(attackIndex, out AbstractEnemyAttack targetAttack) &&
                   targetAttack.CanAttack(target);
        }

        public void Attack(int attackIndex, GameObject target = null)
        {
            if (_attackDict.TryGetValue(attackIndex, out AbstractEnemyAttack targetAttack))
            {
                OwnerEnemy.PlayAnimation(targetAttack.AttackData.AnimatorParam);
                targetAttack.Attack(target);
            }
        }

        public void StopAttack(int attackIndex)
        {
            if (_attackDict.TryGetValue(attackIndex, out AbstractEnemyAttack targetAttack))
                targetAttack.StopAttack();
        }

        /// <summary> 모든 공격의 쿨타임을 초기화합니다. </summary>
        public void ResetAllCooldowns()
        {
            foreach (AbstractEnemyAttack attack in _attackDict.Values)
                attack.ResetCooldown();
        }

        public float GetBaseDamage(AttackDataSO attackData)
        {
            return _currentDamage * attackData.DamageMultiplier;
        }

        public void SetLastBattle()
            => _lastAttackTime = Time.time;

        public void ClearLastBattle()
            => _lastAttackTime = float.NegativeInfinity;

        /// <summary> 전투 시간을 다시 세고 공격이 끝났음을 알립니다. </summary>
        public void InvokeAttackEnd()
        {
            SetLastBattle();
            OnAttackEnd?.Invoke();
        }
    }
}
