using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.Inputs
{
    /// <summary>
    /// 화면 위치가 버튼 · 창 같은 UI 위인지 확인합니다. 카메라 드래그 영역(TouchLookArea)은 UI로 치지 않습니다.
    /// </summary>
    public static class UIPointerChecker
    {
        private static readonly List<RaycastResult> _results = new();
        private static PointerEventData _pointerData;
        private static EventSystem _pointerEventSystem;

        /// <summary> 화면 좌표가 UI 위인지 확인합니다. </summary>
        // EventSystem.IsPointerOverGameObject는 화면 절반을 덮는 카메라 드래그 영역까지 UI로 잡으므로 직접 레이캐스트한다
        public static bool IsOverUI(Vector2 screenPosition)
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
                return false;

            // 씬이 바뀌어 EventSystem이 달라지면 새로 만든다
            if (_pointerData == null || _pointerEventSystem != eventSystem)
            {
                _pointerData = new PointerEventData(eventSystem);
                _pointerEventSystem = eventSystem;
            }

            _pointerData.position = screenPosition;
            _results.Clear();
            eventSystem.RaycastAll(_pointerData, _results);

            foreach (RaycastResult result in _results)
            {
                if (result.gameObject.GetComponentInParent<TouchLookArea>() == null)
                    return true;
            }

            return false;
        }
    }
}
