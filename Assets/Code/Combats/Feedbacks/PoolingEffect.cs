using System.Collections;
using Code.Pools;
using UnityEngine;

namespace Code.Combats.Feedbacks
{
    /// <summary>
    /// 풀에서 꺼내 쓰는 이펙트입니다. 수명이 지나면 스스로 풀에 돌아갑니다. 이펙트 프리팹 루트에 붙입니다.
    /// </summary>
    public class PoolingEffect : MonoBehaviour, IPoolable
    {
        [field: SerializeField] public PoolingItemSO PoolingType { get; private set; }

        public GameObject GameObject
            => gameObject;

        private Pool _myPool;
        private WaitForSeconds _lifetimeWait;
        private float _lifetimeSeconds = -1f;

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        // 파티클이 Play On Awake라 다시 켜지면 처음부터 재생되므로 따로 할 일이 없다
        public void ResetItem()
        { }

        public void CleanUpItem()
        { }

        /// <summary>
        /// lifetime초 뒤에 풀로 돌아갑니다. 꺼낸 직후 호출합니다.
        /// </summary>
        public void PushAfter(float lifetime)
        {
            // 이펙트마다 수명이 고정이라 사실상 처음 한 번만 만든다
            if (_lifetimeWait == null || !Mathf.Approximately(_lifetimeSeconds, lifetime))
            {
                _lifetimeSeconds = lifetime;
                _lifetimeWait = new WaitForSeconds(lifetime);
            }

            StartCoroutine(PushRoutine());
        }

        // 지역 이동으로 먼저 회수되면 꺼지면서 코루틴도 멈춰, 다시 쓰는 중인 이펙트를 빼앗지 않는다
        private IEnumerator PushRoutine()
        {
            yield return _lifetimeWait;
            _myPool.Push(this);
        }
    }
}
