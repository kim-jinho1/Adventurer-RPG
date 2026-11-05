using System.Collections.Generic;
using UnityEngine;

namespace Code.Pools
{
    /// <summary>
    /// 아이템마다 풀을 만들어 두고 꺼내기 · 돌려놓기를 맡는 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "PoolManager", menuName = "SO/Pool/Manager", order = 0)]
    public class PoolManagerSO : ScriptableObject
    {
        [SerializeField] private List<PoolingItemSO> itemList = new();

        // 아이템마다 풀을 하나씩 만든다
        private Dictionary<PoolingItemSO, Pool> _pools;
        private Transform _rootTrm;

        public bool IsInitialized
            => _pools != null;

        public void Initialize(Transform rootTrm)
        {
            _rootTrm = rootTrm;
            _pools = new Dictionary<PoolingItemSO, Pool>();

            foreach (PoolingItemSO item in itemList)
            {
                IPoolable poolable = item.Prefab.GetComponent<IPoolable>();
                Debug.Assert(poolable != null, $"[PoolManagerSO] {item.name} 프리팹에 IPoolable 컴포넌트 없음: {name}", this);

                // 프리팹이 다른 PoolingItemSO를 가리키면 Push가 엉뚱한 풀로 돌아간다
                Debug.Assert(poolable == null || poolable.PoolingType == item,
                    $"[PoolManagerSO] {item.name} 프리팹이 다른 PoolingItemSO를 가리킴: {name}", this);

                GameObject newParent = new GameObject(item.name);
                newParent.transform.SetParent(_rootTrm);

                Pool pool = new Pool(item, newParent.transform);
                _pools.Add(item, pool);
            }
        }

        /// <summary> 만든 풀을 모두 잊습니다. </summary>
        // SO는 Play를 꺼도 메모리에 남으므로 지난 Play의 풀을 비운다
        public void Clear()
        {
            _pools = null;
            _rootTrm = null;
        }

        /// <summary> 아이템의 풀에서 하나 꺼냅니다. </summary>
        public T Pop<T>(PoolingItemSO findItem, Vector3 position, Quaternion rotation) where T : class
        {
            if (_pools.TryGetValue(findItem, out Pool pool))
                return pool.Pop(position, rotation) as T;

            return null;
        }

        /// <summary> 자기 풀로 돌려놓습니다. </summary>
        public void Push(IPoolable item)
        {
            if (_pools.TryGetValue(item.PoolingType, out Pool pool))
                pool.Push(item);
        }

        /// <summary> 모든 풀에서 꺼낸 것을 돌려놓습니다. </summary>
        public void PushAll()
        {
            foreach (Pool pool in _pools.Values)
                pool.PushAll();
        }
    }
}
