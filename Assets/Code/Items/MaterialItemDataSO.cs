using UnityEngine;

namespace Code.Items
{
    /// <summary>
    /// 제작에 쓰이는 재료 아이템입니다. (가죽, 돌 조각, 철광석, 목재, 약초 등)
    /// </summary>
    [CreateAssetMenu(fileName = "MaterialItem", menuName = "SO/Item/Material item", order = 1)]
    public class MaterialItemDataSO : AbstractItemDataSO
    {
        public override ItemTypeEnum ItemType
            => ItemTypeEnum.Material;
    }
}
