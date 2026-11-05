using System.Collections.Generic;
using Code.Craftings;
using Code.Items;
using UnityEngine;

namespace Code.UIs
{
    /// <summary>
    /// 제작 창의 레시피 목록입니다. 칸을 그리고, 칸을 누르면 몇 번째를 골랐는지 알립니다.
    /// </summary>
    public class RecipeListView : MonoBehaviour
    {
        [Header("Recipe List")]
        [SerializeField] private ItemSlotView recipeTemplate;
        [SerializeField] private Transform recipeContainer;

        /// <summary> 레시피 선택 델리게이트입니다. </summary>
        public delegate void RecipeSelectHandler(int index);
        /// <summary> 레시피 칸을 누르면 호출됩니다. 무엇을 할지는 이 뷰를 쓰는 창이 정합니다. </summary>
        public event RecipeSelectHandler OnRecipeSelected;

        private readonly List<ItemSlotView> _recipeViews = new();

        private void Awake()
        {
            Debug.Assert(recipeTemplate != null, $"[RecipeListView] recipeTemplate 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(recipeContainer != null, $"[RecipeListView] recipeContainer 연결 안 됨: {gameObject.name}", this);
            if (recipeTemplate != null)
                recipeTemplate.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            foreach (ItemSlotView recipeView in _recipeViews)
            {
                if (recipeView != null)
                    recipeView.OnSlotClicked -= HandleSlotClicked;
            }
        }

        /// <summary>
        /// 레시피 목록을 그리고 고른 칸을 강조합니다.
        /// </summary>
        public void Show(RecipeListSO recipeList, int selectedIndex)
        {
            int recipeCount = recipeList.Recipes.Count;
            BuildViews(recipeCount);

            for (int i = 0; i < recipeCount; i++)
            {
                RecipeSO recipe = recipeList.Recipes[i];
                ItemSlotView recipeView = _recipeViews[i];
                AbstractItemDataSO resultItem = recipe != null ? recipe.Result.Item : null;
                int resultCount = recipe != null ? recipe.Result.Count : 0;

                recipeView.Show(resultItem, resultCount > 1 ? resultCount.ToString() : string.Empty);
                recipeView.SetLabel(resultItem != null ? resultItem.ItemName : string.Empty);
                recipeView.SetSelected(i == selectedIndex);
            }
        }

        private void BuildViews(int recipeCount)
        {
            // 제작대마다 레시피 수가 달라 모자란 만큼만 새로 만들고 남는 칸은 숨긴다
            while (_recipeViews.Count < recipeCount)
            {
                ItemSlotView recipeView = Instantiate(recipeTemplate, recipeContainer);
                recipeView.SetIndex(_recipeViews.Count);
                recipeView.OnSlotClicked += HandleSlotClicked;
                _recipeViews.Add(recipeView);
            }

            for (int i = 0; i < _recipeViews.Count; i++)
                _recipeViews[i].gameObject.SetActive(i < recipeCount);
        }

        private void HandleSlotClicked(ItemSlotView slotView)
            => OnRecipeSelected?.Invoke(slotView.Index);
    }
}
