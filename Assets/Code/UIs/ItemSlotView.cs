using Code.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UIs
{
    /// <summary>
    /// 아이템 한 칸을 그리는 뷰입니다. 인벤토리 칸 · 장비 슬롯 · 레시피 목록 · 재료 표시가 같이 씁니다.
    /// </summary>
    public class ItemSlotView : MonoBehaviour
    {
        [Header("Slot View")]
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private GameObject selectedMark;

        /// <summary> 칸 클릭 델리게이트입니다. </summary>
        public delegate void SlotClickHandler(ItemSlotView slotView);
        /// <summary> 칸을 누르면 호출됩니다. </summary>
        public event SlotClickHandler OnSlotClicked;

        /// <summary> 목록 안에서의 번호입니다. 인벤토리 칸 번호 · 레시피 번호로 씁니다. </summary>
        public int Index { get; private set; }

        public AbstractItemDataSO Item { get; private set; }

        private void Awake()
        {
            if (button != null)
                button.onClick.AddListener(HandleButtonClick);
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(HandleButtonClick);
        }

        /// <summary> 목록 번호를 정합니다. 생성 직후 호출합니다. </summary>
        public void SetIndex(int index)
        {
            Index = index;
        }

        /// <summary> 아이템과 개수 글자를 표시합니다. item이 null이면 빈 칸입니다. </summary>
        public void Show(AbstractItemDataSO item, string countLabel)
        {
            Item = item;
            if (iconImage != null)
            {
                iconImage.enabled = item != null && item.Icon != null;
                iconImage.sprite = item != null ? item.Icon : null;
            }

            if (countText != null)
                countText.text = countLabel;
        }

        /// <summary> 빈 칸으로 만듭니다. </summary>
        public void Clear()
            => Show(null, string.Empty);

        /// <summary> 이름 등 보조 글자를 표시합니다. </summary>
        public void SetLabel(string label)
        {
            if (labelText != null)
                labelText.text = label;
        }

        /// <summary> 개수 글자 색을 바꿉니다. (재료 부족 표시 등) </summary>
        public void SetCountColor(Color color)
        {
            if (countText != null)
                countText.color = color;
        }

        /// <summary> 선택 표시를 켜거나 끕니다. </summary>
        public void SetSelected(bool isSelected)
        {
            if (selectedMark != null)
                selectedMark.SetActive(isSelected);
        }

        private void HandleButtonClick()
        {
            OnSlotClicked?.Invoke(this);
        }
    }
}
