using Code.Modules;

namespace Code.Interactions
{
    /// <summary>
    /// 상호작용 키로 사용할 수 있는 월드 오브젝트입니다. (제작대, 채집 노드 등)
    /// </summary>
    public interface IInteractable
    {
        /// <summary> 지금 상호작용할 수 있는지 확인합니다. </summary>
        bool CanInteract(ModuleOwner interactor);
        /// <summary> 상호작용합니다. interactor의 모듈을 받아 아이템 지급 · UI 열기 등을 합니다. </summary>
        void Interact(ModuleOwner interactor);
    }
}
