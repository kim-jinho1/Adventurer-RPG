using Code.Items;

namespace Code.Inventories
{
    /// <summary>
    /// 가방 칸의 아이템 행동(사용 · 장착)을 대신 부르는 모듈 인터페이스입니다.
    /// </summary>
    public interface IItemUseModule
    {
        /// <summary> 아이템 사용 델리게이트입니다. </summary>
        delegate void ItemUseHandler(AbstractItemDataSO item);
        /// <summary> 아이템을 써서 1개가 줄었을 때 호출됩니다. 장착은 부르지 않습니다. </summary>
        event ItemUseHandler OnItemUsed;

        /// <summary> 이 칸의 행동 버튼을 지금 누를 수 있는지 확인합니다. </summary>
        bool CanActAt(ItemTypeEnum itemType, int slotIndex);
        /// <summary> 이 칸의 아이템 행동을 합니다. 소모품이면 1개를 뺍니다. </summary>
        bool ActAt(ItemTypeEnum itemType, int slotIndex);
    }
}
