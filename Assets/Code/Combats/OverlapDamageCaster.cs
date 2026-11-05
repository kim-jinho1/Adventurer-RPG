using System.Collections.Generic;
using UnityEngine;

namespace Code.Combats
{
    /// <summary>
    /// 범위 내의 모든 대상에게 피해를 입히는 캐스터입니다.
    /// </summary>
    public class OverlapDamageCaster : AbstractDamageCaster
    {
        [Header("Overlap Damage Caster Settings")]
        [SerializeField] private CastTypeEnum castType;
        [SerializeField] private float radius;
        [SerializeField] private Vector3 boxSize;
        [SerializeField] private Vector3 hitPointOffset = new(0, 1f, 0);

        // 한 대상에 콜라이더가 여러 개여도 한 번 휘두를 때 한 번만 맞게 하는 목록 (매 공격 할당을 피하려고 재사용)
        private readonly HashSet<IDamageable> _hitTargets = new();

        /// <summary> 공격 범위를 설정합니다. </summary>
        public void SetRadius(float value)
            => radius = value;

        /// <summary>
        /// 공격을 시전합니다.
        /// </summary>
        /// <param name="damage">입히는 데미지</param>
        /// <param name="knockBackForce">밀쳐내는 힘</param>
        /// <param name="isCritical">크리티컬 여부</param>
        /// <param name="causesStagger">맞은 대상을 경직시키는지</param>
        /// <returns>공격 성공 여부</returns>
        public override bool CastDamage(float damage, Vector3 knockBackForce, bool isCritical, bool causesStagger)
        {
            int cnt = castType switch
            {
                CastTypeEnum.Sphere => Physics.OverlapSphereNonAlloc(
                    transform.position, radius, _hitResults, targetLayer, triggerInteraction),
                CastTypeEnum.Box => Physics.OverlapBoxNonAlloc(
                    transform.position, boxSize * 0.5f, _hitResults, transform.rotation, targetLayer, triggerInteraction),
                _ => 0
            };

            if (cnt == 0)
                return false;

            Vector3 direction = Owner.transform.forward;
            Vector3 worldKnockBackForce = Owner.transform.TransformDirection(knockBackForce);

            _hitTargets.Clear();
            for (int i = 0; i < cnt; i++)
            {
                if (!_hitResults[i].TryGetComponent(out IDamageable damageable))
                    continue;

                if (!_hitTargets.Add(damageable))
                    continue;

                Vector3 closestPoint = _hitResults[i].ClosestPoint(transform.position);
                Vector3 toCaster = transform.position - closestPoint;
                Vector3 normal = toCaster.sqrMagnitude > Mathf.Epsilon ? toCaster.normalized : -direction;
                Vector3 point = closestPoint + hitPointOffset;

                DamageData damageData = new DamageData
                {
                    DamageAmount = damage,
                    Dealer = Owner,
                    IsCritical = isCritical,
                    CausesStagger = causesStagger,
                    KnockBackForce = worldKnockBackForce
                };

                damageable.ApplyDamage(damageData, point, direction, normal);
            }

            return true;
        }

        /// <summary>
        /// 공격 범위를 시각화합니다.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            if (castType == CastTypeEnum.Sphere)
            {
                Gizmos.DrawWireSphere(transform.position, radius);
            }
            else if (castType == CastTypeEnum.Box)
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, boxSize);
                Gizmos.matrix = Matrix4x4.identity;
            }
        }
    }
}
