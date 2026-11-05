using Code.CoreSystem.Bus;

namespace Code.Craftings
{
    /// <summary>
    /// 제작대와 상호작용했을 때 제작 UI를 열라고 알리는 버스 이벤트입니다.
    /// </summary>
    public readonly struct CraftingStationOpenEvent : IEvent
    {
        /// <summary> 열린 제작대가 보여 줄 레시피 목록입니다. </summary>
        public RecipeListSO RecipeList { get; }

        /// <summary> 제작을 실행할 상호작용한 쪽의 제작 모듈입니다. </summary>
        public ICraftingModule CraftingModule { get; }

        /// <summary> 창을 연 제작대입니다. 제작이 끝나면 이 제작대로 망치질합니다. </summary>
        public CraftingStation CraftingStation { get; }

        public CraftingStationOpenEvent(RecipeListSO recipeList, ICraftingModule craftingModule, CraftingStation craftingStation)
        {
            RecipeList = recipeList;
            CraftingModule = craftingModule;
            CraftingStation = craftingStation;
        }
    }
}
