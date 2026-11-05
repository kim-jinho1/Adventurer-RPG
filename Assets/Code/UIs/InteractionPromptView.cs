using Code.Agents.Characters;
using Code.Interactions;
using Code.Modules;
using TMPro;
using UnityEngine;

namespace Code.UIs
{
    /// <summary>
    /// 플레이어에게 가장 가까운 상호작용 대상 위에 "[E] 제작" 같은 말풍선을 띄웁니다.
    /// 화면 UI라 거리와 상관없이 글자가 또렷하고, 창이 열려 있거나 대상이 카메라 뒤에 있으면 숨깁니다.
    /// </summary>
    public class InteractionPromptView : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private ModuleOwner player;
        [SerializeField] private CharacterInputSO characterInput;

        [Header("View")]
        [SerializeField] private RectTransform promptRoot;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private string keyHint = "E";      // 모바일은 화면 버튼을 누르므로 키 표시를 뺀다

        private IInteractionModule _interactionModule;
        private InteractableMarker _target;
        private Camera _camera;

        private void Start()
        {
            Debug.Assert(promptRoot != null, $"[InteractionPromptView] promptRoot 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(promptText != null, $"[InteractionPromptView] promptText 연결 안 됨: {gameObject.name}", this);

            _interactionModule = player != null ? player.GetModule<IInteractionModule>() : null;
            Debug.Assert(_interactionModule != null, $"[InteractionPromptView] player에 IInteractionModule 모듈 없음: {gameObject.name}", this);

            if (_interactionModule != null)
                _interactionModule.OnFocusChanged += HandleFocusChanged;

            HandleFocusChanged(_interactionModule != null ? _interactionModule.FocusedMarker : null);
        }

        private void OnDestroy()
        {
            if (_interactionModule != null)
                _interactionModule.OnFocusChanged -= HandleFocusChanged;
        }

        private void LateUpdate()
        {
            // 지역이 바뀌어 대상이 사라지면 Unity의 null 비교로 걸러진다
            bool isBlocked = characterInput != null && characterInput.IsGameplayBlocked;
            if (_target == null || isBlocked || !TryGetCamera(out Camera mainCamera))
            {
                SetVisible(false);
                return;
            }

            Vector3 screenPoint = mainCamera.WorldToScreenPoint(_target.PromptPosition);
            if (screenPoint.z <= 0f)
            {
                SetVisible(false);
                return;
            }

            // Overlay 캔버스는 화면 픽셀 좌표가 곧 위치다
            promptRoot.position = new Vector3(screenPoint.x, screenPoint.y, 0f);
            SetVisible(true);
        }

        private void HandleFocusChanged(InteractableMarker marker)
        {
            _target = marker;
            if (_target == null)
            {
                SetVisible(false);
                return;
            }

            promptText.text = Application.isMobilePlatform || string.IsNullOrEmpty(keyHint)
                ? _target.Label
                : $"[{keyHint}] {_target.Label}";
        }

        private bool TryGetCamera(out Camera mainCamera)
        {
            // 씬이 바뀌면 카메라가 바뀔 수 있어, 비어 있을 때만 다시 찾는다
            if (_camera == null)
                _camera = Camera.main;

            mainCamera = _camera;
            return mainCamera != null;
        }

        private void SetVisible(bool isVisible)
        {
            if (promptRoot != null && promptRoot.gameObject.activeSelf != isVisible)
                promptRoot.gameObject.SetActive(isVisible);
        }
    }
}
