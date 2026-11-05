using System;
using UnityEngine;

namespace Code.Items.Drops
{
    /// <summary>
    /// 드랍 테이블의 한 줄입니다. 확률을 통과하면 MinCount ~ MaxCount개가 나옵니다.
    /// </summary>
    [Serializable]
    public class DropEntry
    {
        [field: SerializeField] public AbstractItemDataSO Item { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float Chance { get; private set; } = 1f;
        [field: SerializeField, Min(0)] public int MinCount { get; private set; } = 1;
        [field: SerializeField, Min(0)] public int MaxCount { get; private set; } = 1;

        /// <summary> 최소 · 최대가 뒤집히지 않게 맞춥니다. 테이블의 OnValidate에서 호출합니다. </summary>
        public void Validate()
        {
            if (MaxCount < MinCount)
                MaxCount = MinCount;
        }
    }
}
