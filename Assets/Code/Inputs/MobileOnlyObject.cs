using UnityEngine;

namespace Code.Inputs
{
    /// <summary>
    /// 모바일 기기에서만 오브젝트를 켜 둡니다. PC 빌드에서는 시작할 때 끕니다. 화면 조이스틱 · 버튼 묶음(MobileControls)에 붙입니다.
    /// </summary>
    public class MobileOnlyObject : MonoBehaviour
    {
        [SerializeField] private bool isShowInEditor = true;     // 에디터에서 터치 버튼으로 시험할 수 있게 기본은 보인다

        private void Awake()
        {
            if (!IsMobile())
                gameObject.SetActive(false);
        }

        private bool IsMobile()
        {
#if UNITY_EDITOR
            return isShowInEditor || Application.isMobilePlatform;
#else
            return Application.isMobilePlatform;
#endif
        }
    }
}
