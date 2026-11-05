using System.Collections;
using Code.Agents.Characters;
using Code.CoreSystem.Bus;
using Code.GameEvents.Worlds;
using Code.GameSystems.Saves;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Code.UIs
{
    /// <summary>
    /// 마지막 보스를 쓰러뜨리면 잠시 뒤 화면을 덮는 엔딩 화면입니다. [타이틀로]를 누르면 Core 씬을 다시 불러와 타이틀로 돌아갑니다.
    /// </summary>
    public class EndingPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterInputSO characterInput;
        [SerializeField] private SaveManager saveManager;

        [Header("UI")]
        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private Button titleButton;
        [SerializeField] private CanvasGroup[] hiddenOnEnding;      // 엔딩 동안 숨길 HUD · 화면 버튼

        [Header("Ending Setting")]
        [SerializeField, Min(0f)] private float showDelay = 3f;          // 보스가 쓰러지는 모습을 보여 주는 시간
        [SerializeField, Min(0.01f)] private float fadeDuration = 1.5f;

        [Header("Event")]
        [SerializeField] private OwnerEnum eventOwner = OwnerEnum.Player;

        private BusSubscriptionScope _busScope;
        private bool _isShown;

        private void Awake()
        {
            Debug.Assert(characterInput != null, $"[EndingPanel] characterInput 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(panelGroup != null, $"[EndingPanel] panelGroup 연결 안 됨: {gameObject.name}", this);

            _busScope = new BusSubscriptionScope();
            _busScope.Subscribe<GameClearedEvent>(eventOwner, HandleGameCleared);
            titleButton.onClick.AddListener(HandleTitleClicked);

            panelGroup.gameObject.SetActive(false);
        }

        // 씬을 다시 불러올 때 입력 차단이 SO에 남아 있지 않게 푼다
        private void OnDestroy()
        {
            _busScope?.Dispose();
            titleButton.onClick.RemoveListener(HandleTitleClicked);

            if (characterInput != null)
                characterInput.SetGameplayBlock(this, false);
        }

        private void HandleGameCleared(GameClearedEvent evt)
        {
            if (_isShown)
                return;

            _isShown = true;
            StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            yield return new WaitForSecondsRealtime(showDelay);

            characterInput.SetGameplayBlock(this, true);
            panelGroup.gameObject.SetActive(true);
            panelGroup.interactable = false;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / fadeDuration);
                panelGroup.alpha = progress;
                SetHudAlpha(1f - progress);
                yield return null;
            }

            panelGroup.alpha = 1f;
            panelGroup.interactable = true;
            SetHudAlpha(0f);
        }

        private void SetHudAlpha(float alpha)
        {
            foreach (CanvasGroup hud in hiddenOnEnding)
            {
                if (hud == null)
                    continue;

                hud.alpha = alpha;
                hud.blocksRaycasts = alpha > 0f;
            }
        }

        private void HandleTitleClicked()
        {
            titleButton.interactable = false;

            // 보스를 잡고 얻은 경험치 · 아이템을 남기고 돌아간다
            if (saveManager != null)
                saveManager.Save();

            SceneManager.LoadScene(gameObject.scene.name);
        }
    }
}
