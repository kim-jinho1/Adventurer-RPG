namespace Code.Craftings
{
    /// <summary>
    /// 제작 가능 여부 · 제작 결과입니다. UI가 버튼 상태와 안내 문구를 고를 때 씁니다.
    /// </summary>
    public enum CraftResultEnum
    {
        Success = 0,
        InvalidRecipe = 1,
        NotEnoughMaterials = 2,
        NotEnoughSpace = 3,
    }
}
