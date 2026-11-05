using UnityEngine;
using UnityEngine.UI;

namespace Code.UIs
{
    /// <summary>
    /// 바(체력 · 경험치) 채움 이미지를 다루는 확장 메서드입니다.
    /// </summary>
    public static class ImageExtensions
    {
        /// <summary>
        /// 채움 이미지의 오른쪽 끝을 비율만큼 옮겨 바를 채웁니다. 0이면 숨깁니다.
        /// </summary>
        public static void SetBarRatio(this Image fill, float ratio)
        {
            // Filled 타입은 9분할을 무시해 둥근 끝이 네모가 되므로, 9분할 그림을 그대로 두고 너비를 줄인다
            ratio = Mathf.Clamp01(ratio);
            fill.enabled = ratio > 0f;
            RectTransform rect = fill.rectTransform;
            rect.anchorMax = new Vector2(ratio, rect.anchorMax.y);
        }
    }
}
