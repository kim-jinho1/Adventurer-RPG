using UnityEngine;

namespace Code.Pools
{
    /// <summary>
    /// 풀에서 꺼내고 돌려놓을 수 있는 오브젝트 인터페이스입니다.
    /// </summary>
    public interface IPoolable
    {
        /// <summary> 어느 풀 아이템인지 </summary>
        public PoolingItemSO PoolingType { get; }
        /// <summary> 풀이 켜고 끌 게임 오브젝트 </summary>
        public GameObject GameObject { get; }
        /// <summary> 돌아갈 풀을 받습니다. 처음 만들 때 한 번 호출됩니다. </summary>
        void SetUpPool(Pool pool);
        /// <summary> 풀에서 꺼낼 때 상태를 처음으로 되돌립니다. </summary>
        void ResetItem();
        /// <summary> 풀로 돌아가기 전에 정리합니다. </summary>
        void CleanUpItem();
    }
}
