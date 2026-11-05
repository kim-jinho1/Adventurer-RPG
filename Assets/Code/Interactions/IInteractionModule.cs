using System;

namespace Code.Interactions
{
    /// <summary>
    /// 상호작용 모듈의 인터페이스
    /// </summary>
    public interface IInteractionModule
    {
        event Action<InteractableMarker> OnFocusChanged; // 외곽선 · 말풍선 대상이 바뀌면 호출됩니다 (없어지면 null)

        InteractableMarker FocusedMarker { get; } // 지금 외곽선 · 말풍선을 보여 주는 대상. 없으면 null

        IInteractable FindClosestInteractable(); // 주변에서 상호작용할 수 있는 가장 가까운 대상. 없으면 null
        void InteractImmediately(IInteractable interactable); // 작업 모션 없이 바로 상호작용합니다. (포탈 · 제작대 창 열기 등)
    }
}
