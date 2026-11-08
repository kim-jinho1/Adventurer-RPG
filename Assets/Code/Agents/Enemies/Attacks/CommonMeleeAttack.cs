using Code.Combats;
using Unity.Profiling;
using UnityEngine;

namespace Code.Agents.Enemies.Attacks
{
    /// <summary>
    /// 적의 기본 근접 공격 클래스입니다. 클립의 AttackTrigger 순간에 데미지 캐스터로 데미지를 줍니다.
    /// </summary>
    public class CommonMeleeAttack : AbstractEnemyAttack
    {
        private static readonly ProfilerMarker _castDamageMarker = new("CommonMeleeAttack.HandleCastDamage");

        private AbstractDamageCaster _damageCaster;

        public override void InitializeAttack(EnemyAttackModule attackModule)
        {
            base.InitializeAttack(attackModule);

            _damageCaster = GetComponentInChildren<AbstractDamageCaster>();
            Debug.Assert(_damageCaster != null, $"[CommonMeleeAttack] AbstractDamageCaster 컴포넌트 없음: {gameObject.name}", this);

            _damageCaster.InitCaster(_enemy);
        }

        public override bool CanAttack(GameObject target = null)
        {
            if (target == null)
                return false;

            float sqrDistance = _enemy.GetFlatSqrDistance(target.transform.position);
            return sqrDistance < AttackData.Range * AttackData.Range && NormalizedCooldown >= 1f;
        }

        public override void Attack(GameObject target = null)
        {
            SubscribeTriggers();
        }

        public override void StopAttack()
        {
            UnSubscribeTriggers();
        }

        protected override void HandleAnimationEnd()
        {
            UnSubscribeTriggers();
            _lastAttackTime = Time.time;
            _attackModule.InvokeAttackEnd();
        }

        protected override void HandleCastDamage()
        {
            using (_castDamageMarker.Auto())
            {
                float damage = _attackModule.GetBaseDamage(AttackData);
                Vector3 knockBackPower = AttackData.KnockBackForce;
                _damageCaster.CastDamage(damage, knockBackPower, false, AttackData.CausesStagger);
            }
        }
    }
}
