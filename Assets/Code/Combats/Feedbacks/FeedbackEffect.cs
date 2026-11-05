using Code.Pools;
using UnityEngine;

namespace Code.Combats.Feedbacks
{
    /// <summary>
    /// 타격 이펙트를 풀에서 꺼내 띄우고 수명이 지나면 돌려놓습니다. 풀이 없는 씬에서는 만들고 부숩니다.
    /// </summary>
    public static class FeedbackEffect
    {
        /// <summary> 풀에서 이펙트를 꺼내 놓고 lifetime 뒤 돌려놓습니다. 풀이 없으면 새로 만들고 lifetime 뒤 지웁니다. </summary>
        public static void Spawn(PoolManagerSO poolManager, PoolingItemSO effectItem,
            Vector3 position, Quaternion rotation, float lifetime)
        {
            if (effectItem == null)
                return;

            if (poolManager != null && poolManager.IsInitialized)
            {
                PoolingEffect effect = poolManager.Pop<PoolingEffect>(effectItem, position, rotation);
                if (effect != null)
                {
                    effect.PushAfter(lifetime);
                    return;
                }
            }

            // Core 씬이 없는 테스트 씬이거나 풀 목록에 없는 이펙트
            GameObject spawned = Object.Instantiate(effectItem.Prefab, position, rotation);
            Object.Destroy(spawned, lifetime);
        }
    }
}
