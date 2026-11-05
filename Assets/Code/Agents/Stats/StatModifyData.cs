using System;
using UnityEngine;

namespace Code.Agents.Stats
{
    /// <summary>
    /// 어떤 스탯을 얼마나 바꿀지 담는 데이터입니다. 장비 보너스 · 레벨업 증가량이 같이 씁니다.
    /// </summary>
    [Serializable]
    public struct StatModifyData
    {
        [field: SerializeField] public StatSO TargetStat { get; private set; }
        [field: SerializeField] public float ModifyValue { get; private set; }
    }
}
