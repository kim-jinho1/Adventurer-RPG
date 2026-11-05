using Code.Modules;
using UnityEngine;

namespace Code.Combats
{
    /// <summary>
    /// 공격을 시전하는 추상 클래스입니다.
    /// </summary>
    public abstract class AbstractDamageCaster : MonoBehaviour
    {
        [Header("Damage Caster Settings")]
        [SerializeField] protected int maxHitCount = 5;
        [SerializeField] protected LayerMask targetLayer;
        [SerializeField] protected QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        public ModuleOwner Owner { get; private set; }

        protected Collider[] _hitResults;

        /// <summary> 공격하는 쪽을 정하고 맞은 대상 버퍼를 만듭니다. </summary>
        public void InitCaster(ModuleOwner owner)
        {
            Owner = owner;
            _hitResults = new Collider[maxHitCount];
        }

        /// <summary>
        /// 공격을 시전합니다.
        /// </summary>
        /// <param name="damage"> 입히는 데미지 </param>
        /// <param name="knockBackForce"> 밀쳐내는 힘 </param>
        /// <param name="isCritical"> 크리티컬 여부 </param>
        /// <param name="causesStagger"> 맞은 대상을 경직시키는지 </param>
        /// <returns> 공격 성공 여부 </returns>
        public abstract bool CastDamage(float damage, Vector3 knockBackForce, bool isCritical, bool causesStagger);
    }
}
