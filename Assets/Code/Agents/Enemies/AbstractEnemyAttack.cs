using Code.Combats;
using UnityEngine;

namespace Code.Agents.Enemies
{
    /// <summary>
    /// 적 공격의 추상 클래스입니다. 공격 데이터와 쿨타임을 가지고, 실제 공격 방식은 자식 클래스가 정합니다.
    /// </summary>
    public abstract class AbstractEnemyAttack : MonoBehaviour, IAttack
    {
        [field: SerializeField] public AttackDataSO AttackData { get; private set; }

        public float NormalizedCooldown
            => Mathf.Clamp01((Time.time - _lastAttackTime) / AttackData.Cooldown);

        protected EnemyAttackModule _attackModule;
        protected AbstractEnemy _enemy;

        protected float _lastAttackTime;

        public virtual void InitializeAttack(IAttackModule attackModule)
        {
            _attackModule = attackModule as EnemyAttackModule;
            Debug.Assert(_attackModule != null, $"[AbstractEnemyAttack] EnemyAttackModule에 붙어 있지 않음: {gameObject.name}", this);

            _enemy = _attackModule.Owner as AbstractEnemy;
            Debug.Assert(_enemy != null, $"[AbstractEnemyAttack] AbstractEnemy에 붙어 있지 않음: {gameObject.name}", this);
        }

        public abstract bool CanAttack(GameObject target = null);

        public abstract void Attack(GameObject target = null);

        public virtual void StopAttack()
        { }

        /// <summary>
        /// 쿨다운 리셋
        /// </summary>
        public void ResetCooldown()
            => _lastAttackTime = Time.time;
    }
}
