using System.Collections;
using Code.Agents.Characters;
using Code.GameSystems.Saves;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UIs
{
    /// <summary>
    /// 게임을 켜면 마을을 배경으로 보여 주는 타이틀 화면입니다. Core 씬 위에 덮어 두고, 시작하면 걷어 냅니다.
    /// 새 게임은 성별을 고르는 화면을 거치고, 이어하기는 저장을 불러와 바로 시작합니다.
    /// </summary>
    public class TitleScreen : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterInputSO characterInput;
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private Character player;

        [Header("Camera")]
        [SerializeField] private CinemachineCamera titleCamera;     // 마을 풍경을 비추는 카메라
        [SerializeField] private CinemachineCamera genderCamera;    // 성별을 고를 때 플레이어 정면을 비추는 카메라

        [Header("Panels")]
        [SerializeField] private CanvasGroup titleGroup;            // 타이틀 UI 전체
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject genderPanel;
        [SerializeField] private CanvasGroup[] hiddenDuringTitle;   // 타이틀 동안 숨길 HUD · 화면 버튼

        [Header("Menu Buttons")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button quitButton;

        [Header("Gender Buttons")]
        [SerializeField] private Button femaleButton;
        [SerializeField] private Button maleButton;
        [SerializeField] private Button startButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Color selectedColor = new(1f, 0.85f, 0.45f, 1f);
        [SerializeField] private Color unselectedColor = Color.white;

        [Header("Fade Setting")]
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.6f;

        private CharacterAppearanceModule _appearance;
        private bool _isStarted;

        private void Awake()
        {
            Debug.Assert(characterInput != null, $"[TitleScreen] characterInput 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(saveManager != null, $"[TitleScreen] saveManager 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(player != null, $"[TitleScreen] player 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(titleGroup != null, $"[TitleScreen] titleGroup 연결 안 됨: {gameObject.name}", this);

            continueButton.onClick.AddListener(HandleContinueClicked);
            newGameButton.onClick.AddListener(HandleNewGameClicked);
            quitButton.onClick.AddListener(HandleQuitClicked);
            femaleButton.onClick.AddListener(HandleFemaleClicked);
            maleButton.onClick.AddListener(HandleMaleClicked);
            startButton.onClick.AddListener(HandleStartClicked);
            backButton.onClick.AddListener(HandleBackClicked);
        }

        // 플레이어 모듈은 플레이어 Awake에서 초기화되므로 Start에서 가져온다
        private void Start()
        {
            _appearance = player != null ? player.GetModule<CharacterAppearanceModule>() : null;
            Debug.Assert(_appearance != null, $"[TitleScreen] player에 CharacterAppearanceModule 모듈 없음: {gameObject.name}", this);

            characterInput.SetGameplayBlock(this, true);
            SetHudVisible(false);

            titleGroup.gameObject.SetActive(true);
            titleGroup.alpha = 1f;
            titleGroup.interactable = true;
            ShowMenu();
        }

        // 씬을 다시 불러올 때 입력 차단이 SO에 남아 있지 않게 푼다
        private void OnDestroy()
        {
            continueButton.onClick.RemoveListener(HandleContinueClicked);
            newGameButton.onClick.RemoveListener(HandleNewGameClicked);
            quitButton.onClick.RemoveListener(HandleQuitClicked);
            femaleButton.onClick.RemoveListener(HandleFemaleClicked);
            maleButton.onClick.RemoveListener(HandleMaleClicked);
            startButton.onClick.RemoveListener(HandleStartClicked);
            backButton.onClick.RemoveListener(HandleBackClicked);

            if (characterInput != null)
                characterInput.SetGameplayBlock(this, false);
        }

        private void ShowMenu()
        {
            menuPanel.SetActive(true);
            genderPanel.SetActive(false);
            SetCameraActive(titleCamera, true);
            SetCameraActive(genderCamera, false);

            // 저장이 없으면 이어할 것도 없다
            continueButton.gameObject.SetActive(saveManager != null && saveManager.HasSaveFile);
        }

        private void ShowGenderSelect()
        {
            menuPanel.SetActive(false);
            genderPanel.SetActive(true);
            SetCameraActive(genderCamera, true);

            if (_appearance != null)
                SelectGender(_appearance.Gender);
        }

        private void SelectGender(GenderEnum gender)
        {
            if (_appearance != null)
                _appearance.SetGender(gender);

            femaleButton.targetGraphic.color = gender == GenderEnum.Female ? selectedColor : unselectedColor;
            maleButton.targetGraphic.color = gender == GenderEnum.Male ? selectedColor : unselectedColor;
        }

        private void StartGame(bool isContinue)
        {
            // 버튼을 여러 번 눌러도 한 번만 시작한다
            if (_isStarted)
                return;

            _isStarted = true;
            titleGroup.interactable = false;
            saveManager.StartSession(isContinue);
            StartCoroutine(HideRoutine());
        }

        private IEnumerator HideRoutine()
        {
            // 타이틀 카메라를 끄면 CinemachineBrain이 플레이어 카메라로 부드럽게 넘어간다
            SetCameraActive(titleCamera, false);
            SetCameraActive(genderCamera, false);

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / fadeDuration);
                titleGroup.alpha = 1f - progress;
                SetHudAlpha(progress);
                yield return null;
            }

            SetHudVisible(true);
            titleGroup.gameObject.SetActive(false);
            characterInput.SetGameplayBlock(this, false);
        }

        private void SetHudVisible(bool isVisible)
        {
            SetHudAlpha(isVisible ? 1f : 0f);
            foreach (CanvasGroup hud in hiddenDuringTitle)
            {
                if (hud == null)
                    continue;

                hud.interactable = isVisible;
                hud.blocksRaycasts = isVisible;
            }
        }

        private void SetHudAlpha(float alpha)
        {
            foreach (CanvasGroup hud in hiddenDuringTitle)
            {
                if (hud != null)
                    hud.alpha = alpha;
            }
        }

        private static void SetCameraActive(CinemachineCamera cinemachineCamera, bool isActive)
        {
            if (cinemachineCamera != null)
                cinemachineCamera.gameObject.SetActive(isActive);
        }

        private void HandleContinueClicked()
        {
            StartGame(true);
        }

        private void HandleNewGameClicked()
        {
            ShowGenderSelect();
        }

        private void HandleQuitClicked()
        {
            Application.Quit();
        }

        private void HandleFemaleClicked()
        {
            SelectGender(GenderEnum.Female);
        }

        private void HandleMaleClicked()
        {
            SelectGender(GenderEnum.Male);
        }

        private void HandleStartClicked()
        {
            StartGame(false);
        }

        private void HandleBackClicked()
        {
            ShowMenu();
        }
    }
}
