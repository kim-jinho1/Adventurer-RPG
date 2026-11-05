using Code.CoreSystem.Bus;
using Code.Craftings;
using Code.Inventories;
using Code.Items;
using Code.Modules;
using TMPro;
using UnityEngine;

namespace Code.UIs
{
    /// <summary>
    /// 제작대와 상호작용하면 열리는 제작 창입니다. 레시피 목록 → 필요 재료(보유/필요) → 제작 버튼 순서로 보여 줍니다.
    /// </summary>
    public class CraftingPanel : AbstractPanel
    {
        [Header("Reference")]
        [SerializeField] private ModuleOwner player;
        [SerializeField] private OwnerEnum eventOwner = OwnerEnum.Player;

        [Header("View")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private RecipeListView recipeListView;
        [SerializeField] private RecipeDetailView recipeDetailView;

        private const int _noSelection = -1;

        private BusSubscriptionScope _busScope;
        private IInventoryModule _inventoryModule;
        private ICraftingModule _craftingModule;
        private CraftingStation _craftingStation;
        private RecipeListSO _recipeList;
        private int _selectedRecipeIndex = _noSelection;

        private void Start()
        {
            Debug.Assert(player != null, $"[CraftingPanel] player 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(recipeListView != null, $"[CraftingPanel] recipeListView 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(recipeDetailView != null, $"[CraftingPanel] recipeDetailView 연결 안 됨: {gameObject.name}", this);
            _inventoryModule = player != null ? player.GetModule<IInventoryModule>() : null;

            _busScope = new BusSubscriptionScope();
            _busScope.Subscribe<CraftingStationOpenEvent>(eventOwner, HandleStationOpen);

            if (recipeListView != null)
                recipeListView.OnRecipeSelected += HandleRecipeSelected;

            if (recipeDetailView != null)
                recipeDetailView.OnCraftClicked += HandleCraftClicked;

            if (_inventoryModule != null)
                _inventoryModule.OnInventoryChanged += HandleInventoryChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _busScope?.Dispose();

            if (recipeListView != null)
                recipeListView.OnRecipeSelected -= HandleRecipeSelected;

            if (recipeDetailView != null)
                recipeDetailView.OnCraftClicked -= HandleCraftClicked;

            if (_inventoryModule != null)
                _inventoryModule.OnInventoryChanged -= HandleInventoryChanged;
        }

        private void HandleStationOpen(CraftingStationOpenEvent evt)
        {
            // 열린 채로 같은 제작대에 또 상호작용하면 고른 레시피가 처음으로 돌아가므로 무시한다
            if (IsOpen && evt.CraftingStation == _craftingStation)
                return;

            // 제작 망치질을 마치고 같은 제작대가 다시 열리면 고른 레시피를 이어 간다
            bool isSameStation = evt.CraftingStation == _craftingStation;
            _recipeList = evt.RecipeList;
            _craftingModule = evt.CraftingModule;
            _craftingStation = evt.CraftingStation;
            if (_recipeList == null || _craftingModule == null)
                return;

            if (titleText != null)
                titleText.text = string.IsNullOrEmpty(_recipeList.ListName) ? "제작" : _recipeList.ListName;

            if (!isSameStation || _selectedRecipeIndex >= _recipeList.Recipes.Count)
                _selectedRecipeIndex = _recipeList.Recipes.Count > 0 ? 0 : _noSelection;

            SetOpen(true);
            RefreshRecipes();
            RefreshDetail();
        }

        private void RefreshRecipes()
        {
            if (_recipeList != null)
                recipeListView.Show(_recipeList, _selectedRecipeIndex);
        }

        private void RefreshDetail()
        {
            RecipeSO recipe = GetSelectedRecipe();
            if (recipe == null)
            {
                recipeDetailView.Clear();
                SetStatus(string.Empty);
                return;
            }

            CraftResultEnum craftable = _craftingModule.CheckCraftable(recipe);
            recipeDetailView.Show(recipe, _inventoryModule, craftable == CraftResultEnum.Success);

            // 재료를 모아 제작할 수 있게 되면 "재료가 부족합니다"도 지워지도록 매번 다시 쓴다
            SetStatus(GetResultMessage(craftable));
        }

        private RecipeSO GetSelectedRecipe()
        {
            if (_recipeList == null || _craftingModule == null)
                return null;

            if (_selectedRecipeIndex < 0 || _selectedRecipeIndex >= _recipeList.Recipes.Count)
                return null;

            return _recipeList.Recipes[_selectedRecipeIndex];
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }

        private static string GetResultMessage(CraftResultEnum result)
        {
            switch (result)
            {
                case CraftResultEnum.NotEnoughMaterials:
                    return "재료가 부족합니다";
                case CraftResultEnum.NotEnoughSpace:
                    return "가방에 빈 칸이 없습니다";
                case CraftResultEnum.InvalidRecipe:
                    return "잘못된 레시피입니다";
                default:
                    return string.Empty;
            }
        }

        private void HandleRecipeSelected(int index)
        {
            _selectedRecipeIndex = index;
            RefreshRecipes();
            RefreshDetail();
        }

        private void HandleCraftClicked()
        {
            RecipeSO recipe = GetSelectedRecipe();
            if (recipe == null)
                return;

            // 상세는 가방 변경 이벤트가, 완료 알림은 망치질이 끝난 뒤 ToastView가 띄운다
            CraftResultEnum result = _craftingModule.Craft(recipe, _craftingStation);
            if (result != CraftResultEnum.Success)
                return;

            // 망치질이 보이도록 창을 닫는다 (끝까지 마치면 제작대가 다시 연다)
            SetOpen(false);
        }

        private void HandleInventoryChanged(ItemTypeEnum itemType)
        {
            if (IsOpen)
                RefreshDetail();
        }
    }
}
