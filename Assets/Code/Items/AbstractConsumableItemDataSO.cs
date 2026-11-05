using Code.Modules;

namespace Code.Items
{
    /// <summary>
    /// 사용하면 사라지는 소모품의 기반입니다. 효과마다 자식 클래스를 만듭니다.
    /// </summary>
    public abstract class AbstractConsumableItemDataSO : AbstractItemDataSO, IItemAction
    {
        public override ItemTypeEnum ItemType
            => ItemTypeEnum.Consumable;

        public string ActionLabel
            => "사용";

        public bool IsConsumedOnAct
            => true;

        /// <summary> 지금 쓸 수 있는지 확인합니다. (예: 체력이 가득이면 회복 포션을 막는다) </summary>
        public virtual bool CanUse(ModuleOwner user)
            => user != null;

        /// <summary> 효과를 적용합니다. 개수 차감은 호출하는 쪽(아이템 사용 모듈)이 합니다. </summary>
        public abstract void Use(ModuleOwner user);

        public bool CanAct(ModuleOwner user, int slotIndex)
            => CanUse(user);

        public bool Act(ModuleOwner user, int slotIndex)
        {
            Use(user);
            return true;
        }
    }
}
