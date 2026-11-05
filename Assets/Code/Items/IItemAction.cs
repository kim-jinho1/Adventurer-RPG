using Code.Modules;

namespace Code.Items
{
    /// <summary>
    /// 가방 창에서 행동 버튼(사용 · 장착)이 있는 아이템입니다.
    /// 버튼 이름과 누르면 할 일을 아이템이 정하고, 아이템 쓰기 모듈이 부릅니다.
    /// </summary>
    public interface IItemAction
    {
        /// <summary> 버튼 글자입니다. (사용, 장착) </summary>
        string ActionLabel { get; }
        /// <summary> 누르면 이 칸에서 1개가 사라지는지입니다. 소모품은 true, 장비는 false입니다. </summary>
        bool IsConsumedOnAct { get; }

        /// <summary> 지금 누를 수 있는지 확인합니다. </summary>
        bool CanAct(ModuleOwner user, int slotIndex);
        /// <summary> 행동을 합니다. 개수 차감은 IsConsumedOnAct를 보고 모듈이 합니다. </summary>
        bool Act(ModuleOwner user, int slotIndex);
    }
}
