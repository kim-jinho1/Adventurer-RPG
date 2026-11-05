using Ami.BroAudio;
using Code.Combats.Feedbacks;
using Code.CoreSystem.Bus;
using Code.GameEvents.Feedbacks;
using Code.Interactions;
using Code.Modules;
using UnityEngine;

namespace Code.Craftings
{
    /// <summary>
    /// 월드에 놓이는 제작대입니다. 상호작용하면 버스로 제작 UI를 열게 하고, 실제 제작은 상호작용한 쪽의 제작 모듈이 합니다.
    /// 제작 후 망치질이 모루에 닿을 때마다 카메라를 흔듭니다.
    /// </summary>
    public class CraftingStation : MonoBehaviour, IInteractable, IWorkHitReceiver
    {
        [field: Header("Crafting Station Setting")]
        [field: SerializeField] public RecipeListSO RecipeList { get; private set; }
        [SerializeField] private OwnerEnum eventOwner = OwnerEnum.Player;

        [Header("Work Hit Feedback")]
        [SerializeField] private ShakeDataSO workHitShake;
        [SerializeField] private SoundID workHitSound;     // 망치가 모루에 닿는 소리

        private void Awake()
        {
            Debug.Assert(RecipeList != null, $"[CraftingStation] RecipeList 연결 안 됨: {gameObject.name}", this);
        }

        public bool CanInteract(ModuleOwner interactor)
            => RecipeList != null && interactor != null && interactor.GetModule<ICraftingModule>() != null;

        public void Interact(ModuleOwner interactor)
        {
            if (!CanInteract(interactor))
                return;

            CraftingStationOpenEvent openEvent = new CraftingStationOpenEvent(RecipeList, interactor.GetModule<ICraftingModule>(), this);
            Bus<CraftingStationOpenEvent>.Raise(eventOwner, openEvent);
        }

        public void HandleWorkHit(ModuleOwner worker)
        {
            if (workHitSound.IsValid())
                BroAudio.Play(workHitSound, transform.position + Vector3.up);

            if (workHitShake != null)
                Bus<CameraShakeEvent>.Raise(eventOwner, new CameraShakeEvent(workHitShake));
        }
    }
}
