using System.Collections.Generic;
using UnityEngine;

namespace Code.Pools
{
    /// <summary>
    /// 프리팹 하나의 오브젝트 풀입니다. 꺼낸 것을 기억해 두었다가 돌려받고, 최대 개수를 넘으면 파괴합니다.
    /// </summary>
    public class Pool
    {
        private readonly Stack<IPoolable> _pool;
        private readonly HashSet<IPoolable> _active;
        private readonly List<IPoolable> _pushBuffer = new();
        private readonly Transform _parentTrm;
        private readonly GameObject _prefab;
        private readonly int _maxCount;

        public Pool(PoolingItemSO item, Transform parentTrm)
        {
            _pool = new Stack<IPoolable>(item.InitCount);
            _active = new HashSet<IPoolable>();
            _parentTrm = parentTrm;
            _prefab = item.Prefab;
            _maxCount = item.MaxCount;

            for (int i = 0; i < item.InitCount; i++)
                _pool.Push(Create()); // 미리 만들어 꺼 둔 채로 넣어 준다
        }

        /// <summary> 하나 꺼내(없으면 새로 만들어) 자리에 놓고 켭니다. </summary>
        public IPoolable Pop(Vector3 position, Quaternion rotation)
        {
            IPoolable item = _pool.Count > 0 ? _pool.Pop() : Create();

            // 켜지는 순간 파티클이 재생되므로 자리를 먼저 잡는다
            item.GameObject.transform.SetPositionAndRotation(position, rotation);
            item.GameObject.SetActive(true);
            _active.Add(item);

            item.ResetItem();
            return item;
        }

        /// <summary> 풀로 돌려놓습니다. 최대 개수를 넘으면 파괴합니다. </summary>
        public void Push(IPoolable item)
        {
            if (!_active.Remove(item))
                return; // 이미 돌아온 것은 두 번 넣지 않는다

            item.CleanUpItem();
            item.GameObject.SetActive(false);

            if (_pool.Count < _maxCount)
                _pool.Push(item);
            else
                Object.Destroy(item.GameObject);
        }

        /// <summary> 꺼낸 것을 모두 돌려놓습니다. </summary>
        public void PushAll()
        {
            // Push가 _active를 지우므로 복사본으로 돈다
            _pushBuffer.AddRange(_active);
            foreach (IPoolable item in _pushBuffer)
                Push(item);

            _pushBuffer.Clear();
        }

        private IPoolable Create()
        {
            GameObject gameObj = Object.Instantiate(_prefab, _parentTrm);
            gameObj.SetActive(false);
            IPoolable item = gameObj.GetComponent<IPoolable>();
            item.SetUpPool(this); // 자기가 돌아갈 풀을 알려 준다
            return item;
        }
    }
}
