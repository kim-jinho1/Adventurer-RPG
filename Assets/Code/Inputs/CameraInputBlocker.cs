using Code.Agents.Characters;
using Unity.Cinemachine;
using UnityEngine;

namespace Code.Inputs
{
    /// <summary>
    /// 가방 · 제작 창이 열려 있는 동안 카메라 회전 입력을 끕니다. 카메라는 입력 에셋의 Look을 직접 읽어 CharacterInputSO의 차단이 닿지 않습니다.
    /// </summary>
    public class CameraInputBlocker : MonoBehaviour
    {
        [SerializeField] private CharacterInputSO characterInput;
        [SerializeField] private CinemachineInputAxisController axisController;

        private void Awake()
        {
            if (axisController == null)
                axisController = GetComponent<CinemachineInputAxisController>();

            Debug.Assert(characterInput != null, $"[CameraInputBlocker] characterInput 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(axisController != null, $"[CameraInputBlocker] CinemachineInputAxisController 컴포넌트 없음: {gameObject.name}", this);
        }

        private void Start()
        {
            if (characterInput == null)
                return;

            characterInput.OnGameplayBlockChanged += HandleGameplayBlockChanged;
            HandleGameplayBlockChanged(characterInput.IsGameplayBlocked);
        }

        // SO의 이벤트는 씬이 바뀌어도 남으므로 반드시 해제한다
        private void OnDestroy()
        {
            if (characterInput != null)
                characterInput.OnGameplayBlockChanged -= HandleGameplayBlockChanged;
        }

        private void HandleGameplayBlockChanged(bool isBlocked)
        {
            if (axisController != null)
                axisController.enabled = !isBlocked;
        }
    }
}
