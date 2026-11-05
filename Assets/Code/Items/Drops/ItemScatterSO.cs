using Code.Pools;
using UnityEngine;

namespace Code.Items.Drops
{
    /// <summary>
    /// 바닥에 아이템을 떨어뜨리는 설정(프리팹 · 높이 · 튕기는 힘)입니다. 적 드랍과 채집 넘침이 같은 에셋을 함께 씁니다.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemScatter", menuName = "SO/Item/Item scatter", order = 11)]
    public class ItemScatterSO : ScriptableObject
    {
        [Header("Scatter Setting")]
        [SerializeField] private PoolManagerSO poolManager;
        [SerializeField] private PoolingItemSO pickupItem;
        [SerializeField] private Vector3 spawnOffset = new(0f, 1f, 0f);
        [SerializeField] private float launchUpForce = 4f;
        [SerializeField] private float launchSideForce = 2f;

        /// <summary>
        /// origin에 spawnOffset을 더한 자리에 아이템 count개를 담은 픽업을 만들어 튕겨 냅니다. 프리팹이 없으면 null을 반환합니다.
        /// </summary>
        public ItemPickup Spawn(Vector3 origin, AbstractItemDataSO item, int count)
        {
            if (pickupItem == null)
                return null;

            Vector3 position = origin + spawnOffset;
            ItemPickup pickUpObject = poolManager != null && poolManager.IsInitialized
                ? poolManager.Pop<ItemPickup>(pickupItem, position, Quaternion.identity)
                : null;

            // Core 씬이 없는 테스트 씬이면 풀 없이 만든다
            if (pickUpObject == null)
                pickUpObject = Instantiate(pickupItem.Prefab, position, Quaternion.identity).GetComponent<ItemPickup>();

            pickUpObject.Setup(item, count);

            // 여러 개가 한 점에 겹치지 않게 옆으로 무작위로 흩뿌린다
            Vector2 side = Random.insideUnitCircle * launchSideForce;
            pickUpObject.Launch(new Vector3(side.x, launchUpForce, side.y));

            return pickUpObject;
        }
    }
}
