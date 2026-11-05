using System.Collections.Generic;
using Code.Agents;
using Code.CoreSystem.Bus;
using Code.Items;
using Code.Modules;
using UnityEngine;

namespace Code.Inventories
{
    /// <summary>
    /// 줍기 · 채집으로 들어온 아이템을 인벤토리에 넣고, 획득 · 실패를 버스로 알립니다.
    /// </summary>
    public class ItemCollectorModule : MonoBehaviour, IModule, IItemCollector
    {
        [Header("Collector Setting")]
        [SerializeField] private Transform collectPoint;
        [SerializeField] private OwnerEnum eventOwner = OwnerEnum.Player;

        public Transform CollectPoint => collectPoint;

        private readonly List<IPickupable> _waitingPickups = new();

        private ModuleOwner _owner;
        private IInventoryModule _inventoryModule;

        private bool _isFullNotified;

        public void Initialize(ModuleOwner owner)
        {
            _owner = owner;
            _inventoryModule = owner.GetModule<IInventoryModule>();
            Debug.Assert(_inventoryModule != null, $"[ItemCollectorModule] IInventoryModule 모듈 없음: {gameObject.name}", this);

            if (collectPoint == null)
                collectPoint = owner.transform;

            _inventoryModule.OnInventoryChanged += HandleInventoryChanged;
        }

        private void OnDestroy()
        {
            if (_inventoryModule != null)
                _inventoryModule.OnInventoryChanged -= HandleInventoryChanged;
        }

        public int Collect(AbstractItemDataSO item, int count)
        {
            if (item == null || count <= 0 || _owner is Agent { IsDead: true })
                return Mathf.Max(count, 0);

            int leftover = _inventoryModule.AddItem(item, count);
            int collected = count - leftover;
            if (collected > 0)
                Bus<ItemAcquiredEvent>.Raise(eventOwner, new ItemAcquiredEvent(item, collected));

            if (leftover > 0)
                NotifyInventoryFull(item);

            return leftover;
        }

        private void HandleInventoryChanged(ItemTypeEnum itemType)
        {
            _isFullNotified = false;

            // 앞에서부터 빼면 뒤 칸이 당겨져 다음 아이템을 건너뛰므로 뒤에서부터 돈다
            for (int i = _waitingPickups.Count - 1; i >= 0; i--)
                if (TryAccept(_waitingPickups[i]))
                    _waitingPickups.RemoveAt(i);
        }

        private void NotifyInventoryFull(AbstractItemDataSO item)
        {
            // 죽어서 못 받는 것은 가방이 찬 것이 아니므로 알리지 않는다
            if (_owner is Agent { IsDead: true } || _isFullNotified)
                return;

            _isFullNotified = true;
            Bus<InventoryFullEvent>.Raise(eventOwner, new InventoryFullEvent(item));
        }

        public void CancelPickup(IPickupable pickup)
        {
            _waitingPickups.Remove(pickup);
            if (_waitingPickups.Count == 0)
                _isFullNotified = false;
        }

        public void RequestPickup(IPickupable pickup)
        {
            if (TryAccept(pickup))
                return;

            _waitingPickups.Add(pickup);
            NotifyInventoryFull(pickup.Item);
        }

        private bool TryAccept(IPickupable pickup)
        {
            if (_owner is Agent { IsDead: true } || !_inventoryModule.CanAddItem(pickup.Item))
                return false;

            pickup.StartFlying();
            return true;
        }
    }
}
