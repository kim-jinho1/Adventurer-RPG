namespace Code.Interactions
{
    /// <summary>
    /// 작업 도구 모듈의 인터페이스
    /// </summary>
    public interface IWorkToolModule
    {
        void Show(WorkTypeSO workType); // 작업 종류에 맞는 도구 하나만 보여 줍니다 (없으면 모두 숨김)
        /// <summary> 모든 작업 도구를 숨깁니다. </summary>
        void HideAll();
    }
}
