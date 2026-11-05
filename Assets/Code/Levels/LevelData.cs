using System;
using UnityEngine;

namespace Code.Levels
{
    /// <summary>
    /// 레벨 하나의 정보입니다. RequiredExp는 이 레벨에서 다음 레벨로 오르는 데 필요한 경험치입니다.
    /// </summary>
    [Serializable]
    public struct LevelData
    {
        [field: SerializeField] public int Level { get; private set; }
        [field: SerializeField, Min(1)] public int RequiredExp { get; private set; }

        public LevelData(int level, int requiredExp)
        {
            Level = level;
            RequiredExp = requiredExp;
        }
    }
}
