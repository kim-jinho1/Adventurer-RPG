using Code.Inventories;
using Code.Items;
using Code.Modules;
using UnityEngine;

namespace Code.Craftings
{
    /// <summary>
    /// 인벤토리 재료로 레시피를 제작합니다. 재료 차감 → 결과 추가 순서이며, 검사를 먼저 해 중간에 실패하지 않습니다.
    /// </summary>
    public class CraftingModule : MonoBehaviour, IModule, ICraftingModule
    {
        public event ICraftingModule.CraftHandler OnCrafted;

        private IInventoryModule _inventoryModule;

        public void Initialize(ModuleOwner owner)
        {
            _inventoryModule = owner.GetModule<IInventoryModule>();
            Debug.Assert(_inventoryModule != null, $"[CraftingModule] IInventoryModule 모듈 없음: {gameObject.name}", this);
        }

        public CraftResultEnum CheckCraftable(RecipeSO recipe)
        {
            if (recipe == null || !recipe.IsValid)
                return CraftResultEnum.InvalidRecipe;

            foreach (ItemAmount ingredient in recipe.Ingredients)
            {
                if (!_inventoryModule.HasItem(ingredient.Item, ingredient.Count))
                    return CraftResultEnum.NotEnoughMaterials;
            }

            // 재료를 빼기 전 기준으로 보므로, 결과가 재료와 같은 탭이고 가방이 꽉 찼으면 실제론 들어가도 막힐 수 있다(보수적)
            if (!_inventoryModule.CanAddItem(recipe.Result.Item, recipe.Result.Count))
                return CraftResultEnum.NotEnoughSpace;

            return CraftResultEnum.Success;
        }

        public bool CanCraft(RecipeSO recipe)
            => CheckCraftable(recipe) == CraftResultEnum.Success;

        public CraftResultEnum Craft(RecipeSO recipe, CraftingStation station)
        {
            CraftResultEnum result = CheckCraftable(recipe);
            if (result != CraftResultEnum.Success)
                return result;

            foreach (ItemAmount ingredient in recipe.Ingredients)
                _inventoryModule.RemoveItem(ingredient.Item, ingredient.Count);

            _inventoryModule.AddItem(recipe.Result.Item, recipe.Result.Count);
            OnCrafted?.Invoke(recipe, station);
            return CraftResultEnum.Success;
        }
    }
}
