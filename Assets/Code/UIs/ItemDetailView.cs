using Code.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UIs
{
    /// <summary>
    /// 선택한 아이템의 그림 · 이름 · 설명과 행동 버튼(사용 · 장착 · 해제 · 제작)을 보여 줍니다.
    /// </summary>
    public class ItemDetailView : MonoBehaviour
    {
        [Header("Detail View")]
        [SerializeField] private GameObject contentRoot;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Button actionButton;
        [SerializeField] private TMP_Text actionText;

        /// <summary> 행동 버튼 클릭 델리게이트입니다. </summary>
        public delegate void ActionClickHandler();
        /// <summary> 행동 버튼을 누르면 호출됩니다. 무엇을 할지는 이 뷰를 쓰는 패널이 정합니다. </summary>
        public event ActionClickHandler OnActionClicked;

        private void Awake()
        {
            if (actionButton != null)
                actionButton.onClick.AddListener(HandleActionClick);
        }

        private void OnDestroy()
        {
            if (actionButton != null)
                actionButton.onClick.RemoveListener(HandleActionClick);
        }

        /// <summary>
        /// 아이템을 표시합니다. actionLabel이 비어 있으면 행동 버튼을 숨깁니다.
        /// </summary>
        public void Show(AbstractItemDataSO item, string actionLabel, bool canAct)
        {
            if (item == null)
            {
                Clear();
                return;
            }

            SetContentActive(true);
            if (iconImage != null)
            {
                iconImage.enabled = item.Icon != null;
                iconImage.sprite = item.Icon;
            }

            if (nameText != null)
                nameText.text = item.ItemName;

            if (descriptionText != null)
                descriptionText.text = item.GetDescription();

            bool hasAction = !string.IsNullOrEmpty(actionLabel);
            if (actionButton != null)
            {
                actionButton.gameObject.SetActive(hasAction);
                actionButton.interactable = canAct;
            }

            if (actionText != null)
                actionText.text = actionLabel;
        }

        /// <summary> 아무것도 선택하지 않은 상태로 비웁니다. </summary>
        public void Clear()
        {
            SetContentActive(false);
        }

        private void SetContentActive(bool isActive)
        {
            if (contentRoot != null)
                contentRoot.SetActive(isActive);
        }

        private void HandleActionClick()
        {
            OnActionClicked?.Invoke();
        }
    }
}
