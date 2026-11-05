using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Agents.Characters
{
    /// <summary>
    /// 플레이어 입력을 이동 · 시점 값과 키 이벤트로 바꿔 주는 ScriptableObject입니다.
    /// 창이 열려 있으면 게임 입력을 막습니다.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterInput", menuName = "SO/CharacterInput", order = 0)]
    public class CharacterInputSO : ScriptableObject, Controls.ICharacterActions
    {
        public Vector2 InputDirection { get; private set; }    // 이동 입력 (WASD · 스틱)
        public Vector2 LookDirection { get; private set; }     // 시점 입력 (마우스 · 오른쪽 스틱 · 터치 드래그)
        // Sprint 키를 누르고 있거나, 아날로그 스틱(패드 · 화면 조이스틱)을 거의 끝까지 밀면 달린다
        public bool IsSprinting
            => _isSprintPressed || (_isAnalogMove && InputDirection.sqrMagnitude >= _analogSprintThreshold * _analogSprintThreshold);

        public event Action OnInteractKeyPressed;
        public event Action OnAttackKeyPressed;
        public event Action OnDodgeKeyPressed;
        public event Action OnInventoryKeyPressed;
        public event Action OnPauseKeyPressed;
        public event Action OnCombatToggleKeyPressed;
        /// <summary> 창이 열리거나 닫혀 게임 입력 차단 여부가 바뀔 때 호출됩니다. 인자는 차단 여부입니다. </summary>
        public event Action<bool> OnGameplayBlockChanged;

        /// <summary> 가방 · 제작 창처럼 화면을 가리는 UI가 열려 이동 · 공격 · 회피 · 시점 입력을 막고 있는지 여부입니다. </summary>
        public bool IsGameplayBlocked
            => _gameplayBlockers.Count > 0;

        /// <summary> 마지막 공격 입력이 마우스에서 들어왔는지 여부입니다. 화면 버튼 공격과 구분해 UI 위 클릭만 걸러 내는 데 씁니다. </summary>
        public bool IsLastAttackFromMouse { get; private set; }

        private const float _analogSprintThreshold = 0.9f;

        private Controls _controls;
        private bool _isSprintPressed;
        private bool _isAnalogMove;     // 키보드는 늘 길이 1이라 스틱일 때만 세기로 달리기를 판단한다

        // 창마다 따로 열고 닫으므로 개수가 아닌 "누가 막는지"로 관리해 중복 · 누락을 막는다
        private readonly HashSet<object> _gameplayBlockers = new();

        private void OnEnable()
        {
            if (_controls == null)
            {
                _controls = new Controls();
                _controls.Character.SetCallbacks(this);
            }

            _controls.Character.Enable();
            // 맵 전체를 켜면 이동도 켜지므로, 창이 열려 있으면 다시 끈다
            if (IsGameplayBlocked)
                _controls.Character.Move.Disable();
        }

        private void OnDisable()
        {
            _controls.Disable();
            _gameplayBlockers.Clear();
        }

        /// <summary>
        /// UI가 열려 있는 동안 이동 · 공격 · 회피 · 시점 입력을 막습니다. 가방 · 일시정지 키는 그대로 받습니다.
        /// </summary>
        public void SetGameplayBlock(object blocker, bool isBlocked)
        {
            bool wasBlocked = IsGameplayBlocked;
            if (isBlocked)
                _gameplayBlockers.Add(blocker);
            else
                _gameplayBlockers.Remove(blocker);

            if (IsGameplayBlocked == wasBlocked)
                return;

            if (IsGameplayBlocked)
            {
                LookDirection = Vector2.zero;
                // 끄면 canceled가 불려 이동 값이 0이 되고, 다시 켜면 initialStateCheck로 누르고 있던 키를 바로 읽는다
                _controls?.Character.Move.Disable();
            }
            else if (_controls != null && _controls.Character.enabled)
            {
                _controls.Character.Move.Enable();
            }

            OnGameplayBlockChanged?.Invoke(IsGameplayBlocked);
        }

        /// <summary> 입력을 켜거나 끕니다. </summary>
        public void SetEnable(bool isEnable)
        {
            if (isEnable)
                OnEnable();
            else
                OnDisable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            InputDirection = context.ReadValue<Vector2>();
            _isAnalogMove = context.control?.device is not Keyboard;
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            // 창 위에서 마우스를 움직일 때 카메라가 돌지 않게 한다
            LookDirection = IsGameplayBlocked ? Vector2.zero : context.ReadValue<Vector2>();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            // 타이틀 · 창이 떠 있을 때 뒤의 채집물 · 제작대와 상호작용하지 않게 한다
            if (context.performed && !IsGameplayBlocked)
                OnInteractKeyPressed?.Invoke();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            // 창을 누른 클릭이 공격으로도 들어가지 않게 한다
            if (!context.performed || IsGameplayBlocked)
                return;

            IsLastAttackFromMouse = context.control?.device is Mouse;
            OnAttackKeyPressed?.Invoke();
        }

        public void OnDodge(InputAction.CallbackContext context)
        {
            if (context.performed && !IsGameplayBlocked)
                OnDodgeKeyPressed?.Invoke();
        }

        public void OnCombatToggle(InputAction.CallbackContext context)
        {
            if (context.performed && !IsGameplayBlocked)
                OnCombatToggleKeyPressed?.Invoke();
        }

        public void OnInventory(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnInventoryKeyPressed?.Invoke();
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnPauseKeyPressed?.Invoke();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            _isSprintPressed = context.ReadValueAsButton();
        }
    }
}
