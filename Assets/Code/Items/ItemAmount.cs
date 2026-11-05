using System;
using UnityEngine;

namespace Code.Items
{
    /// <summary>
    /// 아이템과 개수 한 쌍입니다. 레시피 재료 · 결과, 시작 아이템, 드랍 결과에 씁니다.
    /// </summary>
    [Serializable]
    public struct ItemAmount
    {
        [field: SerializeField] public AbstractItemDataSO Item { get; private set; }
        [field: SerializeField, Min(1)] public int Count { get; private set; }

        public ItemAmount(AbstractItemDataSO item, int count)
        {
            Item = item;
            Count = count;
        }
    }
}
