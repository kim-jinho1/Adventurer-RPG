using Code.Agents;
using Code.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UIs
{
    /// <summary>
    /// 화면 위쪽에 플레이어 체력 바를 표시합니다.
    /// </summary>
    public class HealthHudView : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private ModuleOwner player;

        [Header("View")]
        [SerializeField] private Image healthFillImage;
        [SerializeField] private TMP_Text healthText;

        private IHealthModule _healthModule;

        private void Start()
        {
            Debug.Assert(player != null, $"[HealthHudView] player 연결 안 됨: {gameObject.name}", this);
            _healthModule = player != null ? player.GetModule<IHealthModule>() : null;
            Debug.Assert(_healthModule != null, $"[HealthHudView] player에 IHealthModule 모듈 없음: {gameObject.name}", this);
            if (_healthModule == null)
                return;

            _healthModule.OnHealthChange += HandleHealthChange;
            // 체력은 AfterInitialize에서 이미 채워졌으므로 구독 직후 한 번 그린다
            Refresh(_healthModule.CurrentHealth, _healthModule.MaxHealth);
        }

        private void OnDestroy()
        {
            if (_healthModule == null)
                return;

            _healthModule.OnHealthChange -= HandleHealthChange;
        }

        private void Refresh(float health, float max)
        {
            if (healthFillImage != null)
                healthFillImage.SetBarRatio(max > 0f ? health / max : 0f);

            // 소수 체력이 0.4처럼 남아도 살아 있으면 0으로 보이지 않게 올림한다
            if (healthText != null)
                healthText.text = $"{Mathf.CeilToInt(health)} / {Mathf.CeilToInt(max)}";
        }

        private void HandleHealthChange(float before, float health, float max)
        {
            Refresh(health, max);
        }
    }
}
