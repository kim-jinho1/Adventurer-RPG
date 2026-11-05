using Code.Agents.Characters;
using UnityEngine;

namespace Code.Inputs
{
    /// <summary>
    /// 마우스 커서를 창이 열려 있을 때만 보여 줍니다. 게임 중에는 숨기고 화면 가운데에 묶어 둡니다.
    /// 가방 · 제작 · 타이틀 · 엔딩이 모두 게임 입력 차단을 걸므로 그 신호 하나로 판단합니다.
    /// </summary>
    public class CursorController : MonoBehaviour
    {
        [SerializeField] private CharacterInputSO characterInput;

        private void Awake()
        {
            Debug.Assert(characterInput != null, $"[CursorController] characterInput 연결 안 됨: {gameObject.name}", this);
        }

        private void Start()
        {
            if (characterInput == null)
                return;

            characterInput.OnGameplayBlockChanged += HandleGameplayBlockChanged;
            HandleGameplayBlockChanged(characterInput.IsGameplayBlocked);
        }

        // SO의 이벤트는 씬이 바뀌어도 남으므로 반드시 해제하고, 커서도 원래대로 돌려 둔다
        private void OnDestroy()
        {
            if (characterInput != null)
                characterInput.OnGameplayBlockChanged -= HandleGameplayBlockChanged;

            ApplyCursor(true);
        }

        // 다른 창으로 나갔다 오면 OS가 커서 잠금을 풀어 버리므로 다시 건다
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && characterInput != null)
                ApplyCursor(characterInput.IsGameplayBlocked);
        }

        private void HandleGameplayBlockChanged(bool isBlocked)
        {
            ApplyCursor(isBlocked);
        }

        private static void ApplyCursor(bool isVisible)
        {
            Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isVisible;
        }
    }
}
