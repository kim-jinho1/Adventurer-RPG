using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace Code.Inputs
{
    /// <summary>
    /// 화면 영역을 끈 양을 가상 Pointer의 delta로 보내 카메라 회전(Look) 입력으로 쓰게 하는 클래스입니다.
    /// 처음 닿은 손가락 하나만 따라가므로 조이스틱 · 버튼을 누르는 다른 손가락은 영향을 주지 않습니다.
    /// </summary>
    public class TouchLookArea : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [InputControl(layout = "Vector2")]
        [SerializeField] private string touchControlPath = "<Pointer>/delta";

        // 포인터 번호 규칙은 입력 모듈마다 달라 "없음"을 특정 번호로 표시하지 않고 bool로 따로 둔다
        private bool _isTracking;
        private int _trackingPointerId;

        protected override string controlPathInternal
        {
            get => touchControlPath;
            set => touchControlPath = value;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _isTracking = false;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                _isTracking = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isTracking)
                return;

            _isTracking = true;
            _trackingPointerId = eventData.pointerId;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isTracking || eventData.pointerId != _trackingPointerId)
                return;

            // Pointer delta는 프레임마다 0으로 돌아가고 한 프레임 안의 값은 더해지므로 끌 때만 보내면 된다
            SendValueToControl(eventData.delta);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isTracking && eventData.pointerId == _trackingPointerId)
                _isTracking = false;
        }
    }
}
