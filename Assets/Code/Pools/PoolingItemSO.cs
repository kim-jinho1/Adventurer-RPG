using UnityEngine;

namespace Code.Pools
{
    /// <summary>
    /// 풀링할 프리팹과 미리 만들 개수 · 최대 개수를 정하는 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "PoolingItem", menuName = "SO/Pool/Item", order = 0)]
    public class PoolingItemSO : ScriptableObject
    {
        [field: SerializeField] public GameObject Prefab { get; private set; }
        [field: SerializeField, Min(0)] public int InitCount { get; private set; }
        [field: SerializeField, Min(1)] public int MaxCount { get; private set; } = 20;
    }
}
