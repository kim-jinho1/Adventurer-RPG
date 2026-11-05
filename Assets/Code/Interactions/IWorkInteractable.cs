namespace Code.Interactions
{
    /// <summary>
    /// 작업 상호작용 인터페이스.
    /// </summary>
    public interface IWorkInteractable : IInteractable
    {
        /// <summary> 이 대상에 쓰는 작업 종류 </summary>
        WorkTypeSO WorkType { get; }
    }
}
