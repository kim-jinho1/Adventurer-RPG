using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace Code.Debugs
{
    /// <summary>
    /// 현재 렌더 설정과 FPS를 화면에 띄우고, 업스케일 방식(Auto ↔ FSR)과 블룸(켬 ↔ 끔)을 버튼으로 바꾸는 측정용 오버레이입니다.
    /// </summary>
    public class RenderSettingsOverlay : MonoBehaviour
    {
        [FormerlySerializedAs("show")]
        [SerializeField] private bool isVisible = true;
        [SerializeField] private int fontSize = 28;
        [SerializeField] private float buttonHeight = 100f;

        [Header("Bloom Toggle")]
        [SerializeField] private VolumeProfile[] bloomProfiles;   // 볼륨 에셋을 여러 씬이 같이 쓰므로 나중에 불러오는 지역에도 같은 상태가 적용된다

        private const float _textAreaHeight = 400f;
        private const float _buttonWidth = 500f;
        private const float _buttonGap = 20f;

        private GUIStyle _style;
        private GUIStyle _buttonStyle;
        private float _deltaTime;

        // 에디터 플레이 모드에서 바꾼 값은 에셋 파일에 남으므로, 시작할 때 값을 기억해 두고 끝날 때 되돌린다.
        private UniversalRenderPipelineAsset _urpAsset;
        private UpscalingFilterSelection _originalFilter;
        private readonly List<Bloom> _blooms = new List<Bloom>();
        private readonly List<bool> _originalBloomActive = new List<bool>();

        private bool HasBloom => _blooms.Count > 0;
        private bool IsBloomOn => HasBloom && _blooms[0].active;

        private void Awake()
        {
            _urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (_urpAsset != null)
                _originalFilter = _urpAsset.upscalingFilter;

            if (bloomProfiles == null)
                return;

            foreach (var profile in bloomProfiles)
            {
                if (profile == null || !profile.TryGet(out Bloom bloom))
                    continue;

                _blooms.Add(bloom);
                _originalBloomActive.Add(bloom.active);
            }
        }

        private void OnDestroy()
        {
            if (_urpAsset != null)
                _urpAsset.upscalingFilter = _originalFilter;

            for (int i = 0; i < _blooms.Count; i++)
                _blooms[i].active = _originalBloomActive[i];
        }

        private void Update()
        {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;

            // Input System 전용이라 OnGUI 버튼이 폰 터치를 못 받으므로 눌림을 여기서 직접 확인한다
            if (!isVisible)
                return;

            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame)
                return;

            // Input System 좌표는 아래쪽이 0, OnGUI 좌표는 위쪽이 0이라 y를 뒤집어 비교한다.
            Vector2 position = pointer.position.ReadValue();
            position.y = Screen.height - position.y;

            if (_urpAsset != null && GetButtonRect(0).Contains(position))
                ToggleUpscaling();
            else if (HasBloom && GetButtonRect(1).Contains(position))
                ToggleBloom();
        }

        private Rect GetButtonRect(int index)
            => new Rect(20, 20 + _textAreaHeight + index * (buttonHeight + _buttonGap), _buttonWidth, buttonHeight);

        private void OnGUI()
        {
            if (!isVisible)
                return;

            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = fontSize,
                    normal = { textColor = Color.yellow }
                };
                _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = fontSize };
            }

            GUILayout.BeginArea(new Rect(20, 20, Screen.width - 40, _textAreaHeight));
            GUILayout.Label(BuildText(), _style);
            GUILayout.EndArea();

            // 클릭에 반응하지 않는 상자로 그리기만 하고, 눌림은 Update에서 처리해 한 번 눌러 두 번 바뀌지 않게 한다
            if (_urpAsset != null)
                GUI.Box(GetButtonRect(0), $"Upscale: {_urpAsset.upscalingFilter}", _buttonStyle);
            if (HasBloom)
                GUI.Box(GetButtonRect(1), $"Bloom: {(IsBloomOn ? "On" : "Off")}", _buttonStyle);
        }

        // 에디터 비교 촬영과 같은 조건이 되도록 샤프닝은 FSR 기본값 그대로 둔다
        private void ToggleUpscaling()
        {
            _urpAsset.upscalingFilter = _urpAsset.upscalingFilter == UpscalingFilterSelection.FSR
                ? UpscalingFilterSelection.Auto
                : UpscalingFilterSelection.FSR;
        }

        // 블룸 값(세기 · 해상도 · 반복)은 볼륨 에셋에 저장된 것을 그대로 쓰고, 켬 · 끔만 바꾼다.
        private void ToggleBloom()
        {
            bool turnOn = !IsBloomOn;
            foreach (var bloom in _blooms)
                bloom.active = turnOn;
        }

        private string BuildText()
        {
            int level = QualitySettings.GetQualityLevel();
            string qualityName = QualitySettings.names[level];

            var rp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (rp == null)
            {
                UnityEngine.Debug.LogWarning($"[RenderSettingsOverlay] 렌더 파이프라인이 URP 아님: {gameObject.name}", this);
                return $"Quality: {qualityName}\nRP: (URP 아님)";
            }

            return $"Quality: {qualityName}\n" +
                   $"RP: {rp.name}\n" +
                   $"HDR: {rp.supportsHDR}  MSAA: {rp.msaaSampleCount}x  RenderScale: {rp.renderScale:F2}\n" +
                   $"Upscale: {rp.upscalingFilter}  Bloom: {(IsBloomOn ? "On" : "Off")}\n" +
                   $"ShadowDist: {rp.shadowDistance}  Cascades: {rp.shadowCascadeCount}  Soft: {rp.supportsSoftShadows}\n" +
                   $"MainLightShadowRes: {rp.mainLightShadowmapResolution}\n" +
                   $"FPS: {1f / _deltaTime:F0}  {Screen.width}x{Screen.height}";
        }
    }
}
