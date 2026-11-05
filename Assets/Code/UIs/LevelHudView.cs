using Code.Levels;
using Code.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UIs
{
    /// <summary>
    /// 화면 위쪽에 레벨과 경험치 바를 표시합니다.
    /// </summary>
    public class LevelHudView : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private ModuleOwner player;

        [Header("View")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private Image expFillImage;
        [SerializeField] private TMP_Text expText;

        private ILevelModule _levelModule;

        private void Start()
        {
            Debug.Assert(player != null, $"[LevelHudView] player 연결 안 됨: {gameObject.name}", this);
            _levelModule = player != null ? player.GetModule<ILevelModule>() : null;
            Debug.Assert(_levelModule != null, $"[LevelHudView] player에 ILevelModule 모듈 없음: {gameObject.name}", this);
            if (_levelModule == null)
                return;

            _levelModule.OnExpChanged += HandleExpChanged;
            _levelModule.OnLevelUp += HandleLevelUp;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_levelModule == null)
                return;

            _levelModule.OnExpChanged -= HandleExpChanged;
            _levelModule.OnLevelUp -= HandleLevelUp;
        }

        private void Refresh()
        {
            if (levelText != null)
                levelText.text = $"Lv.{_levelModule.Level}";

            int requiredExp = _levelModule.RequiredExp;
            if (expFillImage != null)
                expFillImage.SetBarRatio(_levelModule.IsMaxLevel || requiredExp <= 0 ? 1f : (float)_levelModule.CurrentExp / requiredExp);

            if (expText != null)
                expText.text = _levelModule.IsMaxLevel ? "MAX" : $"{_levelModule.CurrentExp} / {requiredExp}";
        }

        private void HandleExpChanged(int currentExp, int requiredExp)
        {
            Refresh();
        }

        private void HandleLevelUp(int level)
        {
            Refresh();
        }
    }
}
