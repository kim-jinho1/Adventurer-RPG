using System.Collections;
using Code.Agents.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UIs
{
    /// <summary>
    /// 공격 · 회피 · 상호작용 입력이 들어오면 해당 화면 버튼이 눌렸다 돌아오는 연출을 보여 줍니다.
    /// 화면 버튼을 터치해도, 키보드 · 마우스로 눌러도 같은 입력 이벤트가 오므로 둘 다 반응합니다.
    /// </summary>
    public class ActionButtonFeedback : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private CharacterInputSO characterInput;
        [SerializeField] private ActionButtonEnum action;

        [Header("Feedback Setting")]
        [SerializeField] private Image targetImage;
        [SerializeField, Range(0.5f, 1f)] private float pressedScale = 0.88f;
        [SerializeField] private Color pressedTint = new(0.65f, 0.65f, 0.65f, 1f);
        [SerializeField, Min(0.01f)] private float recoverDuration = 0.15f;

        private Vector3 _originalScale;
        private Color _originalColor;
        private Coroutine _feedbackRoutine;

        private void Awake()
        {
            if (targetImage == null)
                targetImage = GetComponent<Image>();

            Debug.Assert(characterInput != null, $"[ActionButtonFeedback] characterInput 연결 안 됨: {gameObject.name}", this);

            _originalScale = transform.localScale;
            if (targetImage != null)
                _originalColor = targetImage.color;
        }

        private void OnEnable()
        {
            SetSubscribe(true);
        }

        // 꺼질 때 연출이 중간에 멈춰도 원래 모습으로 돌려 둔다
        private void OnDisable()
        {
            SetSubscribe(false);
            _feedbackRoutine = null;
            ApplyPress(0f);
        }

        private void SetSubscribe(bool isSubscribe)
        {
            if (characterInput == null)
                return;

            switch (action)
            {
                case ActionButtonEnum.Attack:
                    if (isSubscribe)
                        characterInput.OnAttackKeyPressed += HandleActionPressed;
                    else
                        characterInput.OnAttackKeyPressed -= HandleActionPressed;
                    break;
                case ActionButtonEnum.Dodge:
                    if (isSubscribe)
                        characterInput.OnDodgeKeyPressed += HandleActionPressed;
                    else
                        characterInput.OnDodgeKeyPressed -= HandleActionPressed;
                    break;
                case ActionButtonEnum.Interact:
                    if (isSubscribe)
                        characterInput.OnInteractKeyPressed += HandleActionPressed;
                    else
                        characterInput.OnInteractKeyPressed -= HandleActionPressed;
                    break;
                case ActionButtonEnum.CombatToggle:
                    if (isSubscribe)
                        characterInput.OnCombatToggleKeyPressed += HandleActionPressed;
                    else
                        characterInput.OnCombatToggleKeyPressed -= HandleActionPressed;
                    break;
            }
        }

        private void HandleActionPressed()
        {
            if (!isActiveAndEnabled)
                return;

            // 연타하면 처음부터 다시 눌린다
            if (_feedbackRoutine != null)
                StopCoroutine(_feedbackRoutine);

            _feedbackRoutine = StartCoroutine(FeedbackRoutine());
        }

        private IEnumerator FeedbackRoutine()
        {
            // 일시정지 중에도 반응하도록 실제 시간으로 돌린다
            float elapsed = 0f;
            while (elapsed < recoverDuration)
            {
                ApplyPress(1f - elapsed / recoverDuration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            ApplyPress(0f);
            _feedbackRoutine = null;
        }

        // pressAmount 1 = 완전히 눌림, 0 = 원래 모습
        private void ApplyPress(float pressAmount)
        {
            transform.localScale = _originalScale * Mathf.Lerp(1f, pressedScale, pressAmount);

            if (targetImage != null)
                targetImage.color = Color.Lerp(_originalColor, _originalColor * pressedTint, pressAmount);
        }
    }
}
