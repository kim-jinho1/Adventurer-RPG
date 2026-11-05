using System.Collections.Generic;
using Code.GameSystems;
using Code.Items;
using UnityEngine;

namespace Code.Craftings
{
    /// <summary>
    /// 제작 레시피입니다. 필요한 재료 목록과 만들어지는 결과 아이템을 담습니다.
    /// </summary>
    [CreateAssetMenu(fileName = "Recipe", menuName = "SO/Crafting/Recipe", order = 0)]
    public class RecipeSO : IndexedAsset
    {
        [field: Header("Recipe Info")]
        [field: SerializeField] public ItemAmount Result { get; private set; }
        [SerializeField] private List<ItemAmount> ingredients = new();

        /// <summary> 필요한 재료 목록입니다. 같은 아이템은 한 줄로만 적습니다. </summary>
        public IReadOnlyList<ItemAmount> Ingredients => ingredients;

        /// <summary> 결과와 재료가 모두 지정되어 제작할 수 있는 레시피인지 확인합니다. </summary>
        public bool IsValid
        {
            get
            {
                if (Result.Item == null || Result.Count <= 0)
                    return false;

                foreach (ItemAmount ingredient in ingredients)
                {
                    if (ingredient.Item == null || ingredient.Count <= 0)
                        return false;
                }

                return true;
            }
        }

        private void OnValidate()
        {
            // 같은 재료가 두 줄이면 줄마다 따로 개수를 확인해 실제 필요량보다 적게 검사된다
            HashSet<AbstractItemDataSO> seen = new();
            foreach (ItemAmount ingredient in ingredients)
            {
                if (ingredient.Item != null && !seen.Add(ingredient.Item))
                    Debug.LogWarning($"[RecipeSO] 재료 {ingredient.Item.name} 중복: {name}", this);
            }
        }
    }
}
