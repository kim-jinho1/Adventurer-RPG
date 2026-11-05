using UnityEngine;

namespace Code.Interactions
{
    /// <summary>
    /// 상호작용 대상에 붙여, 플레이어에게 가장 가까울 때 외곽선을 켜고 말풍선에 쓸 이름 · 위치를 알려 줍니다.
    /// </summary>
    public class InteractableMarker : MonoBehaviour
    {
        [field: Header("Prompt")]
        [field: SerializeField] public string Label { get; private set; } = "상호작용";
        [SerializeField] private Transform promptAnchor;            // 비우면 오브젝트 위치 + promptHeight
        [SerializeField] private float promptHeight = 1.8f;

        [Header("Outline")]
        [SerializeField] private GameObject[] outlineObjects;       // 꺼 둔 InteractOutline 자식들

        public Vector3 PromptPosition
            => promptAnchor != null ? promptAnchor.position : transform.position + Vector3.up * promptHeight;

        private void Awake()
        {
            SetFocused(false);
        }

        /// <summary>
        /// 가장 가까운 대상이 되면 외곽선을 켜고, 벗어나면 끕니다.
        /// </summary>
        public void SetFocused(bool isFocused)
        {
            foreach (GameObject outline in outlineObjects)
            {
                if (outline != null)
                    outline.SetActive(isFocused);
            }
        }
    }
}
