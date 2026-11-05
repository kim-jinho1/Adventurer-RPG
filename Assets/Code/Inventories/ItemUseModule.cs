using Code.Agents;
using Code.Items;
using Code.Modules;
using UnityEngine;

namespace Code.Inventories
{
    /// <summary>
    /// 가방 칸의 아이템 행동(사용 · 장착)을 대신 부릅니다. 할 일은 아이템 SO가 정하고, 소모품이면 이 모듈이 1개를 뺍니다.
    /// </summary>
    public class ItemUseModule : MonoBehaviour, IModule, IItemUseModule
    {
        public event IItemUseModule.ItemUseHandler OnItemUsed;

        private ModuleOwner _owner;
        private IInventoryModule _inventoryModule;

        public void Initialize(ModuleOwner owner)
        {
            _owner = owner;
            _inventoryModule = owner.GetModule<IInventoryModule>();
            Debug.Assert(_inventoryModule != null, $"[ItemUseModule] IInventoryModule 모듈 없음: {gameObject.name}", this);
        }

        public bool CanActAt(ItemTypeEnum itemType, int slotIndex)
        {
            if (_owner is Agent { IsDead: true })
                return false;

            IItemAction action = GetActionAt(itemType, slotIndex);
            return action != null && action.CanAct(_owner, slotIndex);
        }

        public bool ActAt(ItemTypeEnum itemType, int slotIndex)
        {
            if (!CanActAt(itemType, slotIndex))
                return false;

            // 장착은 Act 안에서 칸이 바뀌므로 아이템을 먼저 잡아 둔다
            AbstractItemDataSO item = _inventoryModule.GetSlot(itemType, slotIndex).ItemData;
            IItemAction action = (IItemAction)item;
            if (!action.Act(_owner, slotIndex))
                return false;

            if (action.IsConsumedOnAct)
            {
                _inventoryModule.RemoveItemAt(itemType, slotIndex);
                OnItemUsed?.Invoke(item);
            }

            return true;
        }

        private IItemAction GetActionAt(ItemTypeEnum itemType, int slotIndex)
            => _inventoryModule.GetSlot(itemType, slotIndex)?.ItemData as IItemAction;
    }
}
