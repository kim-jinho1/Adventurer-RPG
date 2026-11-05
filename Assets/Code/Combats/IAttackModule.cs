using System;
using Code.Modules;
using UnityEngine;

namespace Code.Combats
{
    /// <summary>
    /// 공격 모듈 인터페이스입니다.
    /// </summary>
    public interface IAttackModule
    {
        /// <summary> 모듈을 가진 오브젝트 </summary>
        ModuleOwner Owner { get; }
        /// <summary> 전투 상태 여부 (마지막 공격 · 피격 뒤 BattleDuration 동안) </summary>
        bool IsInBattle { get; }

        /// <summary> 공격이 끝나면 호출됩니다. </summary>
        public event Action OnAttackEnd;

        /// <summary> 공격을 시전할 수 있는지 확인합니다. </summary>
        bool CanAttack(int attackIndex, GameObject target = null);
        /// <summary> 공격을 시전합니다. </summary>
        void Attack(int attackIndex, GameObject target = null);
        /// <summary> 진행 중인 공격을 중단합니다. </summary>
        void StopAttack(int attackIndex);
        /// <summary> 공격의 기본 데미지를 가져옵니다. </summary>
        float GetBaseDamage(AttackDataSO attackData);

        /// <summary> 전투 시간을 지금부터 다시 셉니다. </summary>
        void SetLastBattle();
        /// <summary> 전투 상태를 바로 끕니다. </summary>
        void ClearLastBattle();
    }
}
