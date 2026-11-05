using UnityEngine;

namespace Code.Combats
{
    /// <summary>
    /// 공격을 받을 수 있는 인터페이스입니다.
    /// </summary>
    public interface IDamageable
    {
        /// <summary> 데미지를 적용합니다. </summary>
        void ApplyDamage(DamageData damageData, Vector3 hitPoint, Vector3 hitDirection, Vector3 hitNormal);
    }
}