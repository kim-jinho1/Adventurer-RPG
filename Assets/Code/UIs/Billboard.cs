using UnityEngine;

namespace Code.UIs
{
    /// <summary>
    /// 월드에 띄운 그림(떨어진 아이템 아이콘 등)이 항상 카메라를 보게 합니다.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        private Transform _cameraTransform;

        private void Start()
        {
            // 매 프레임 Camera.main을 찾지 않도록 한 번만 가져온다
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                _cameraTransform = mainCamera.transform;
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null)
                return;

            transform.rotation = _cameraTransform.rotation;
        }
    }
}
