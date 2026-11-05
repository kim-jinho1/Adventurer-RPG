using Code.GameSystems;
using UnityEngine;

namespace Code.Items
{
    /// <summary>
    /// 모든 아이템 데이터의 기반입니다. 종류(탭)는 자식 클래스가 정합니다.
    /// </summary>
    public abstract class AbstractItemDataSO : IndexedAsset
    {
        [field: Header("Item Info")]
        [field: SerializeField] public string ItemName { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public int MaxStack { get; protected set; } = 99;
        [field: SerializeField, TextArea] public string Description { get; private set; }

        /// <summary> 아이템 종류입니다. 인벤토리 탭을 정합니다. </summary>
        public abstract ItemTypeEnum ItemType { get; }

        /// <summary> UI에 보여 줄 설명을 만듭니다. 자식 클래스가 스탯 · 효과 설명을 덧붙입니다. </summary>
        public virtual string GetDescription()
            => Description;

        protected virtual void OnValidate()
        {
            // 0 이하면 인벤토리가 칸에 하나도 못 넣어 무한히 빈 칸을 찾는다
            if (MaxStack < 1)
                MaxStack = 1;
        }
    }
}
