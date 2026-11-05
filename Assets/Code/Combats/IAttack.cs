using UnityEngine;

namespace Code.Combats
{
    /// <summary>
    /// 공격 인터페이스입니다.
    /// </summary>
    public interface IAttack
    {
        /// <summary> 공격 데이터 </summary>
        AttackDataSO AttackData { get; }
        /// <summary> 쿨타임 진행률 (0 = 방금 사용, 1 = 준비됨) </summary>
        float NormalizedCooldown { get; }

        /// <summary> 공격을 초기화합니다. </summary>
        void InitializeAttack(IAttackModule attackModule);

        /// <summary> 공격을 시전할 수 있는지 확인합니다. </summary>
        bool CanAttack(GameObject target = null);
        /// <summary> 공격을 시전합니다. </summary>
        void Attack(GameObject target = null);
        /// <summary> 공격을 중단합니다. </summary>
        void StopAttack();
    }
}
