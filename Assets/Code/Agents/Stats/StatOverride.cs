using System;
using UnityEngine;

namespace Code.Agents.Stats
{
    /// <summary>
    /// StatSO를 오버라이드하여 새로운 StatSO를 생성하는 클래스입니다.
    /// </summary>
    [Serializable]
    public class StatOverride
    {
        [field: SerializeField] public StatSO StatData { get; private set; }

        [SerializeField] private bool isUseOverride;
        [SerializeField] private float overrideBaseValue;

        public StatOverride(StatSO stat)
            => StatData = stat;

        /// <summary> StatSO를 오버라이드하여 새로운 StatSO를 생성합니다. </summary>
        public StatSO CreateStat()
        {
            StatSO newStat = StatData.Clone() as StatSO;
            if (isUseOverride && newStat != null)
                    newStat.BaseValue = overrideBaseValue;

            return newStat;
        }
    }
}
