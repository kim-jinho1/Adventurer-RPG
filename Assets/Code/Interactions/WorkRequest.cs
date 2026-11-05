namespace Code.Interactions
{
    /// <summary>
    /// 해당 작업을 나타내는 구조체
    /// </summary>
    public struct WorkRequest
    {
        public IInteractable Target { get; } // 끝까지 마치면 Interact할 대상. null이면 모션만
        public WorkTypeSO WorkType { get; }

        public bool IsValid => WorkType != null;    // 작업 종류가 있으면 실제 요청

        public WorkRequest(IInteractable target, WorkTypeSO workType)
        {
            Target = target;
            WorkType = workType;
        }
    }
}
