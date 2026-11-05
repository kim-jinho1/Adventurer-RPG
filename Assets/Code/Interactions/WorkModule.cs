using Code.Craftings;
using Code.Modules;
using UnityEngine;

namespace Code.Interactions
{
    /// <summary>
    /// 지금 하려는 작업을 들고 있고, 작업 요청을 알립니다
    /// </summary>
    public class WorkModule : MonoBehaviour, IModule, IAfterInitModule, IWorkModule
    {
        [Header("Work Setting")]
        [SerializeField] private WorkTypeSO craftWorkType;

        public event IWorkModule.WorkRequestHandler OnWorkRequested;
        public event IWorkModule.WorkCompleteHandler OnWorkCompleted;

        public WorkRequest CurrentWork { get; private set; }

        private ICraftingModule _craftingModule;

        public bool IsWorking => CurrentWork.IsValid;

        public void Initialize(ModuleOwner owner)
        {
            _craftingModule = owner.GetModule<ICraftingModule>();
            Debug.Assert(_craftingModule != null, $"[WorkModule] ICraftingModule 모듈 없음: {gameObject.name}", this);
            Debug.Assert(craftWorkType != null, $"[WorkModule] craftWorkType 연결 안 됨: {gameObject.name}", this);
        }

        public void AfterInitialize()
        {
            _craftingModule.OnCrafted += HandleCrafted;
        }

        private void OnDestroy()
        {
            _craftingModule.OnCrafted -= HandleCrafted;
        }

        public void RequestWork(IInteractable target, WorkTypeSO workType)
        {
            if(workType == null)
                return;

            CurrentWork = new WorkRequest(target, workType);
            OnWorkRequested?.Invoke(CurrentWork);
        }

        public void CompleteWork(WorkRequest work)
        {
            OnWorkCompleted?.Invoke(work);
        }

        public void ClearWork()
        {
            CurrentWork = default;
        }

        private void HandleCrafted(RecipeSO recipe, CraftingStation station)
        {
            RequestWork(station, craftWorkType);
        }
    }
}