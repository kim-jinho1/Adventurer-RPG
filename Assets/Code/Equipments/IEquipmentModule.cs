using Code.Items;

namespace Code.Equipments
{
    /// <summary>
    /// 장비 장착 모듈 인터페이스입니다. 슬롯(무기 · 방어구)마다 장비 하나를 장착합니다.
    /// </summary>
    public interface IEquipmentModule
    {
        /// <summary> 장비 변경 델리게이트입니다. 해제면 currentItem이, 빈 슬롯 장착이면 prevItem이 null입니다. </summary>
        delegate void EquipmentChangeHandler(EquipSlotEnum slot, EquipItemDataSO prevItem, EquipItemDataSO currentItem);
        /// <summary> 장착 · 해제 · 교체되면 호출됩니다. </summary>
        event EquipmentChangeHandler OnEquipmentChanged;

        /// <summary> 슬롯에 장착된 장비를 가져옵니다. 비었으면 null입니다. </summary>
        EquipItemDataSO GetEquippedItem(EquipSlotEnum slot);
        /// <summary> 이 장비를 장착하고 있는지 확인합니다. </summary>
        bool IsEquipped(EquipItemDataSO item);
        /// <summary> 슬롯의 장비를 벗을 수 있는지 확인합니다. 필수 슬롯(무기)은 교체만 되고, 가방에 자리가 없어도 false입니다. </summary>
        bool CanUnequip(EquipSlotEnum slot);

        /// <summary> 가방 장비 탭 slotIndex 칸의 장비를 장착합니다. 슬롯에 있던 장비는 가방으로 돌아갑니다. </summary>
        bool EquipAt(int slotIndex);
        /// <summary> 장비를 벗어 인벤토리에 넣습니다. CanUnequip이 false면 아무것도 하지 않고 false를 반환합니다. </summary>
        bool Unequip(EquipSlotEnum slot);
    }
}
