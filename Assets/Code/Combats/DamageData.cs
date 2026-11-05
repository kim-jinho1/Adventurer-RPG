using Code.Modules;
using UnityEngine;

namespace Code.Combats
{
    /// <summary>
    /// 한 번의 공격으로 들어가는 데미지 정보입니다.
    /// </summary>
    public struct DamageData
    {
        public float DamageAmount;          // 입히는 데미지
        public bool IsCritical;             // 크리티컬 여부
        public bool CausesStagger;          // 경직 여부
        public Vector3 KnockBackForce;      // 밀쳐내는 힘
        public ModuleOwner Dealer;          // 공격자
    }
}
