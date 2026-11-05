using System.Collections.Generic;
using Code.Agents.Stats;
using UnityEngine;

namespace Code.Levels
{
    /// <summary>
    /// 레벨별 필요 경험치와 레벨업 때 오르는 스탯을 담는 테이블입니다. 마지막 줄의 레벨이 최대 레벨입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelTable", menuName = "SO/Level/Level table", order = 0)]
    public class LevelTableSO : ScriptableObject
    {
        [SerializeField] private List<LevelData> levels = new();
        [SerializeField] private List<StatModifyData> levelUpStats = new();

        /// <summary> 레벨이 1 오를 때마다 기본 스탯에 더할 값입니다. </summary>
        public IReadOnlyList<StatModifyData> LevelUpStats => levelUpStats;

        /// <summary> 최대 레벨입니다. </summary>
        public int MaxLevel
            => Mathf.Max(levels.Count, 1);

        /// <summary> 최대 레벨인지 확인합니다. </summary>
        public bool IsMaxLevel(int level)
            => level >= MaxLevel;

        /// <summary> level에서 다음 레벨로 오르는 데 필요한 경험치입니다. 최대 레벨이면 0입니다. </summary>
        public int GetRequiredExp(int level)
        {
            if (level < 1 || IsMaxLevel(level))
                return 0;

            return levels[level - 1].RequiredExp;
        }

        private void OnValidate()
        {
            // 레벨 번호를 줄 순서로 맞춰 두어 인덱스(level - 1)로 바로 찾게 한다
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].Level != i + 1 || levels[i].RequiredExp < 1)
                    levels[i] = new LevelData(i + 1, Mathf.Max(levels[i].RequiredExp, 1));
            }
        }
    }
}
