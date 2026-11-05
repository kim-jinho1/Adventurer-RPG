using System.Collections.Generic;
using Code.Items;

namespace Code.Inventories
{
    /// <summary>
    /// 격자형 인벤토리 모듈 인터페이스입니다. 아이템 종류(탭)마다 칸이 따로 있습니다.
    /// </summary>
    public interface IInventoryModule
    {
        /// <summary> 인벤토리 변경 델리게이트입니다. 바뀐 탭을 알려 줍니다. </summary>
        delegate void InventoryChangeHandler(ItemTypeEnum itemType);
        /// <summary> 아이템이 추가 · 제거 · 이동되면 호출됩니다. </summary>
        event InventoryChangeHandler OnInventoryChanged;

        /// <summary> 탭의 칸 수를 가져옵니다. </summary>
        int GetSlotCount(ItemTypeEnum itemType);
        /// <summary> 탭의 모든 칸을 가져옵니다. 빈 칸은 null입니다. </summary>
        IReadOnlyList<IReadOnlyInventoryItem> GetSlots(ItemTypeEnum itemType);
        /// <summary> 탭의 특정 칸을 가져옵니다. 비었거나 범위 밖이면 null입니다. </summary>
        IReadOnlyInventoryItem GetSlot(ItemTypeEnum itemType, int slotIndex);

        /// <summary> 가진 개수를 셉니다. </summary>
        int GetItemCount(AbstractItemDataSO item);
        /// <summary> count개 이상 가졌는지 확인합니다. </summary>
        bool HasItem(AbstractItemDataSO item, int count = 1);
        /// <summary> count개를 모두 넣을 공간이 있는지 확인합니다. </summary>
        bool CanAddItem(AbstractItemDataSO item, int count = 1);
        /// <summary> 목록의 아이템을 한꺼번에 모두 넣을 공간이 있는지 확인합니다. 같은 빈 칸을 두 번 세지 않습니다. </summary>
        bool CanAddItems(IReadOnlyList<ItemAmount> items);

        /// <summary> 아이템을 넣습니다. 공간이 모자라 못 넣은 개수를 반환합니다. </summary>
        int AddItem(AbstractItemDataSO item, int count = 1);
        /// <summary> 아이템을 뺍니다. 개수가 모자라면 아무것도 빼지 않고 false를 반환합니다. </summary>
        bool RemoveItem(AbstractItemDataSO item, int count = 1);
        /// <summary> 특정 칸에서 아이템을 뺍니다. 개수가 모자라면 false를 반환합니다. </summary>
        bool RemoveItemAt(ItemTypeEnum itemType, int slotIndex, int count = 1);
        /// <summary> 같은 탭의 두 칸을 바꿉니다. 같은 아이템이면 to 칸으로 합칩니다. </summary>
        void MoveItem(ItemTypeEnum itemType, int fromIndex, int toIndex);
    }
}
