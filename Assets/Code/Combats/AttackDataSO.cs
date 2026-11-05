using System;
using Code.Animations;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Combats
{
    /// <summary>
    /// 공격 데이터를 저장하는 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "Attack data", menuName = "Combat/Attack data", order = 15)]
    public class AttackDataSO : ScriptableObject, ICloneable
    {
        [field: FormerlySerializedAs("AnimatorParam")]
        [field: SerializeField] public AnimParamSO AnimatorParam { get; private set; }

        [field: FormerlySerializedAs("Index")]
        [field: SerializeField] public int Index { get; private set; }                       // 공격 순서 인덱스
        [field: FormerlySerializedAs("AttackName")]
        [field: SerializeField] public string AttackName { get; private set; }               // 공격 이름
        [field: FormerlySerializedAs("Cooldown")]
        [field: SerializeField] public float Cooldown { get; private set; }                  // 공격 쿨타임
        [field: FormerlySerializedAs("Range")]
        [field: SerializeField] public float Range { get; private set; }                     // 공격 범위
        [field: FormerlySerializedAs("DamageMultiplier")]
        [field: SerializeField] public float DamageMultiplier { get; private set; } = 1f;    // 공격 데미지 배율
        [field: FormerlySerializedAs("CausesStagger")]
        [field: SerializeField] public bool CausesStagger { get; private set; }              // 맞은 대상을 경직(HIT)시키는지

        [field: FormerlySerializedAs("KnockBackForce")]
        [field: SerializeField] public Vector3 KnockBackForce { get; private set; }          // 밀쳐내는 힘

        /// <summary>
        /// AttackDataSO를 복제합니다.
        /// </summary>
        public object Clone()
        {
            return Instantiate(this);
        }
    }
}
