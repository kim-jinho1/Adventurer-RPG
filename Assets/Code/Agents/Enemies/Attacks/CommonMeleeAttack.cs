using Code.Animations;
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

        protected IAnimatorTrigger _trigger;
        private AbstractDamageCaster _damageCaster;

        public override void InitializeAttack(IAttackModule attackModule)
        {
            base.InitializeAttack(attackModule);
            _trigger = _enemy.GetModule<IAnimatorTrigger>();
            Debug.Assert(_trigger != null, $"[CommonMeleeAttack] IAnimatorTrigger 모듈 없음: {gameObject.name}", this);

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
            UnSubscribeEvents();
            _trigger.OnAnimationEnd += HandleAttackEnd;
            _trigger.OnAttackTrigger += HandleCastDamage;
        }

        private void HandleAttackEnd()
        {
            UnSubscribeEvents();
            _lastAttackTime = Time.time;
            _attackModule.InvokeAttackEnd();
        }

        private void UnSubscribeEvents()
        {
            _trigger.OnAnimationEnd -= HandleAttackEnd;
            _trigger.OnAttackTrigger -= HandleCastDamage;
        }

        public override void StopAttack()
        {
            UnSubscribeEvents();
        }

        private void HandleCastDamage()
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
