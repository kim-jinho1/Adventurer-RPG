using Code.Agents.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UIs
{
    /// <summary>
    /// 열고 닫는 창의 공통 부모입니다. 열려 있는 동안 이동 · 공격을 막고, 닫기 버튼과 일시정지 키로 닫습니다.
    /// </summary>
    public abstract class AbstractPanel : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private CharacterInputSO characterInput;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;

        public bool IsOpen
            => panelRoot != null && panelRoot.activeSelf;

        protected CharacterInputSO CharacterInput
            => characterInput;

        protected virtual void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(HandleCloseClicked);

            if (characterInput != null)
                characterInput.OnPauseKeyPressed += HandlePauseKey;

            // 자식이 준비되기 전이라 SetOpen 대신 화면만 끈다
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        protected virtual void OnDestroy()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveListener(HandleCloseClicked);

            if (characterInput != null)
            {
                characterInput.OnPauseKeyPressed -= HandlePauseKey;
                characterInput.SetGameplayBlock(this, false);
            }
        }

        /// <summary>
        /// 창을 열거나 닫습니다. 열려 있는 동안 이동 · 공격 입력을 막습니다.
        /// </summary>
        public void SetOpen(bool isOpen)
        {
            if (panelRoot != null)
                panelRoot.SetActive(isOpen);

            if (characterInput != null)
                characterInput.SetGameplayBlock(this, isOpen);

            if (isOpen)
                HandleOpened();
        }

        /// <summary>
        /// 창이 열린 직후 불립니다. 자식이 내용을 새로 그릴 때 override합니다.
        /// </summary>
        protected virtual void HandleOpened()
        { }

        private void HandleCloseClicked()
            => SetOpen(false);

        private void HandlePauseKey()
        {
            if (IsOpen)
                SetOpen(false);
        }
    }
}
