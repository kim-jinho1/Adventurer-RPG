namespace Code.Craftings
{
    /// <summary>
    /// 인벤토리 재료로 레시피를 제작하는 모듈 인터페이스입니다.
    /// </summary>
    public interface ICraftingModule
    {
        /// <summary> 제작 완료 델리게이트입니다. </summary>
        delegate void CraftHandler(RecipeSO recipe, CraftingStation station);
        /// <summary> 제작에 성공하면 호출됩니다. </summary>
        event CraftHandler OnCrafted;

        /// <summary> 제작할 수 있는지, 못 하면 왜인지 알려 줍니다. </summary>
        CraftResultEnum CheckCraftable(RecipeSO recipe);
        /// <summary> 지금 제작할 수 있는지 확인합니다. </summary>
        bool CanCraft(RecipeSO recipe);
        /// <summary> 재료를 빼고 결과 아이템을 넣습니다. 실패하면 아무것도 바뀌지 않습니다. </summary>
        CraftResultEnum Craft(RecipeSO recipe, CraftingStation station);
    }
}
