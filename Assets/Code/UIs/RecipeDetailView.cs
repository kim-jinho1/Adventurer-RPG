using System.Collections.Generic;
using Code.Craftings;
using Code.Inventories;
using Code.Items;
using UnityEngine;

namespace Code.UIs
{
    /// <summary>
    /// 제작 창의 레시피 상세입니다. 결과 아이템과 재료(보유/필요)를 그리고, 제작 버튼이 눌리면 알립니다.
    /// </summary>
    public class RecipeDetailView : MonoBehaviour
    {
        [Header("Recipe Detail")]
        [SerializeField] private ItemDetailView resultDetail;
        [SerializeField] private ItemSlotView ingredientTemplate;
        [SerializeField] private Transform ingredientContainer;

        [Header("Colors")]
        [SerializeField] private Color enoughColor = new(0.93f, 0.9f, 0.69f);
        [SerializeField] private Color lackColor = new(0.95f, 0.35f, 0.3f);

        /// <summary> 제작 버튼 클릭 델리게이트입니다. </summary>
        public delegate void CraftClickHandler();
        /// <summary> 제작 버튼을 누르면 호출됩니다. 제작은 이 뷰를 쓰는 창이 합니다. </summary>
        public event CraftClickHandler OnCraftClicked;

        private readonly List<ItemSlotView> _ingredientViews = new();

        private void Awake()
        {
            Debug.Assert(resultDetail != null, $"[RecipeDetailView] resultDetail 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(ingredientTemplate != null, $"[RecipeDetailView] ingredientTemplate 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(ingredientContainer != null, $"[RecipeDetailView] ingredientContainer 연결 안 됨: {gameObject.name}", this);

            if (ingredientTemplate != null)
                ingredientTemplate.gameObject.SetActive(false);

            if (resultDetail != null)
                resultDetail.OnActionClicked += HandleActionClicked;
        }

        private void OnDestroy()
        {
            if (resultDetail != null)
                resultDetail.OnActionClicked -= HandleActionClicked;
        }

        /// <summary>
        /// 레시피의 결과 아이템과 재료를 그립니다. 보유 개수는 넘겨받은 인벤토리에서 읽습니다.
        /// </summary>
        public void Show(RecipeSO recipe, IInventoryModule inventoryModule, bool canCraft)
        {
            if (resultDetail != null)
                resultDetail.Show(recipe.Result.Item, "제작", canCraft);

            ShowIngredients(recipe, inventoryModule);
        }

        /// <summary>
        /// 고른 레시피가 없을 때 비웁니다.
        /// </summary>
        public void Clear()
        {
            if (resultDetail != null)
                resultDetail.Clear();

            ShowIngredients(null, null);
        }

        private void ShowIngredients(RecipeSO recipe, IInventoryModule inventoryModule)
        {
            int ingredientCount = recipe != null ? recipe.Ingredients.Count : 0;
            while (_ingredientViews.Count < ingredientCount)
                _ingredientViews.Add(Instantiate(ingredientTemplate, ingredientContainer));

            for (int i = 0; i < _ingredientViews.Count; i++)
            {
                ItemSlotView ingredientView = _ingredientViews[i];
                bool isUsed = i < ingredientCount;
                ingredientView.gameObject.SetActive(isUsed);
                if (!isUsed)
                    continue;

                ItemAmount ingredient = recipe.Ingredients[i];
                int ownedCount = inventoryModule?.GetItemCount(ingredient.Item) ?? 0;
                ingredientView.Show(ingredient.Item, $"{ownedCount}/{ingredient.Count}");
                ingredientView.SetCountColor(ownedCount >= ingredient.Count ? enoughColor : lackColor);
                ingredientView.SetLabel(ingredient.Item != null ? ingredient.Item.ItemName : string.Empty);
            }
        }

        private void HandleActionClicked()
            => OnCraftClicked?.Invoke();
    }
}
