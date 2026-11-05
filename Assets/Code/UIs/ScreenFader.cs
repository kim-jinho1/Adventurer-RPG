using System.Collections;
using UnityEngine;

namespace Code.UIs
{
    /// <summary>
    /// 화면 전체를 덮는 검은 판입니다. 지역을 옮기는 동안 화면을 가립니다.
    /// 일시정지 중에도 동작하도록 실제 시간(unscaled)으로 움직입니다.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFader : MonoBehaviour
    {
        [Header("Fade Setting")]
        [SerializeField] private float fadeDuration = 0.4f;

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// 페이드 없이 바로 가리거나 걷습니다. 게임 시작 직후 첫 지역을 불러오기 전에 씁니다.
        /// </summary>
        public void SetCovered(bool isCovered)
        {
            _canvasGroup.alpha = isCovered ? 1f : 0f;
            _canvasGroup.blocksRaycasts = isCovered;
        }

        /// <summary> 화면을 검게 덮습니다. </summary>
        public IEnumerator FadeOut()
            => Fade(1f);

        /// <summary> 화면을 다시 밝힙니다. </summary>
        public IEnumerator FadeIn()
            => Fade(0f);

        private IEnumerator Fade(float targetAlpha)
        {
            // 가리는 동안에는 뒤의 버튼이 눌리지 않게 막는다
            _canvasGroup.blocksRaycasts = true;

            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
            _canvasGroup.blocksRaycasts = targetAlpha > 0f;
        }
    }
}
