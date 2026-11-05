namespace Code.Interactions
{
    /// <summary>
    /// 작업 모듈 인터페이스입니다.
    /// </summary>
    public interface IWorkModule
    {
        /// <summary> 작업 요청 델리게이트입니다. </summary>
        delegate void WorkRequestHandler(WorkRequest request);
        /// <summary> 작업 완료 델리게이트입니다. </summary>
        delegate void WorkCompleteHandler(WorkRequest work);

        event WorkRequestHandler OnWorkRequested;   // 작업을 요청받으면 호출됩니다 (상태 전환은 받는 쪽 Character가 정함)
        event WorkCompleteHandler OnWorkCompleted;  // 작업을 끝까지 마치면 호출됩니다 (취소되면 안 불림)

        /// <summary> 지금 진행 중인 작업 (없으면 IsValid가 false) </summary>
        WorkRequest CurrentWork { get; }

        bool IsWorking { get; } // CurrentWork.IsValid

        /// <summary> 작업을 요청합니다. 실제 시작은 OnWorkRequested를 받은 쪽이 정합니다. </summary>
        void RequestWork(IInteractable target, WorkTypeSO workType);
        /// <summary> 작업을 끝까지 마쳤음을 알립니다. </summary>
        void CompleteWork(WorkRequest work);
        /// <summary> 진행 중인 작업을 비웁니다. </summary>
        void ClearWork();
    }
}
