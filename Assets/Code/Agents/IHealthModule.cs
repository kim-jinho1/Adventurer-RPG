using System;

namespace Code.Agents
{
    /// <summary>
    /// 체력 모듈 인터페이스입니다.
    /// </summary>
    public interface IHealthModule
    {
        /// <summary> 최대 체력 </summary>
        float MaxHealth { get; }
        /// <summary> 현재 체력 </summary>
        float CurrentHealth { get; }
        /// <summary> 무적 여부 </summary>
        bool IsInvincible { get; set; }

        /// <summary> 체력 변경 델리게이트입니다. </summary>
        delegate void HealthChange(float before, float health, float max);
        /// <summary> 체력 변경 알림입니다. </summary>
        event HealthChange OnHealthChange;
        /// <summary> 공격을 받아 체력이 깎였을 때 알립니다. 막타에서도 불립니다. </summary>
        event Action OnDamaged;

        /// <summary> 받은 치유량을 적용합니다. </summary>
        void Heal(float health);
        /// <summary> 체력을 가득 채우고 살아 있는 상태로 되돌립니다. </summary>
        void Revive();
    }
}
