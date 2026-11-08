using System;
using Code.Animations;
using Code.Modules;
using UnityEngine;

namespace Code.Agents
{
    /// <summary>
    /// Agent의 애니메이션을 관리하는 모듈입니다.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AgentRenderer : MonoBehaviour, IModule, IRenderer, IAnimatorTrigger
    {
        [Header("Animation Settings")]
        [SerializeField] private float blendTime = 0.2f;

        [Header("Attack Slots")]
        [SerializeField] private AnimParamSO attackSlotIndexA;
        [SerializeField] private AnimParamSO attackSlotIndexB;
        [SerializeField] private AnimParamSO attackSlotStateA;
        [SerializeField] private AnimParamSO attackSlotStateB;

        public event Action OnAnimationEnd;     // 애니메이션 종료 시 호출되는 이벤트
        public event Action OnAttackTrigger;    // 공격 트리거 시 호출되는 이벤트
        public event Action OnWeaponSwapTrigger; // 무기를 손 ↔ 등으로 옮기는 순간 호출되는 이벤트
        public event Action OnFootstepTrigger;   // 걷기 클립에서 발이 땅에 닿는 순간 호출되는 이벤트

        public int CurrentStateHash
            => _currentStateHash;

        private Agent _owner;
        private Animator _animator;

        private bool _isNextSlotA = true;
        private int _currentStateHash;

        public void Initialize(ModuleOwner owner)
        {
            _owner = owner as Agent;
            _animator = GetComponent<Animator>();
        }

        public void PlayClip(int clipHash, int layer = -1, float normalPosition = Single.NegativeInfinity)
        {
            _currentStateHash = clipHash;
            _animator.Play(clipHash, layer, normalPosition);
        }

        public void PlayAttack(int comboIndex)
        {
            AnimParamSO slotIndex = _isNextSlotA ? attackSlotIndexA : attackSlotIndexB;
            AnimParamSO slotState = _isNextSlotA ? attackSlotStateA : attackSlotStateB;

            // 인덱스를 먼저 넣어야 새 슬롯이 처음부터 올바른 콤보 클립으로 시작한다
            SetAnimFloat(slotIndex, comboIndex);
            CrossFadeState(slotState.HashValue);

            _isNextSlotA = !_isNextSlotA; // 다음 공격 슬롯을 토글
        }

        public void CrossFadeState(int stateHash)
        {
            if (_currentStateHash == stateHash)
                return; // 이미 해당 상태에 있는 경우, 중복 호출 방지

            _currentStateHash = stateHash;
            _animator.CrossFadeInFixedTime(stateHash, blendTime, 0);
        }

        public void CrossFadeState(int stateHash, float transitionDuration, int layer = 0)
        {
            if (_currentStateHash == stateHash)
                return; // 이미 해당 상태에 있는 경우, 중복 호출 방지

            _currentStateHash = stateHash;
            _animator.CrossFadeInFixedTime(stateHash, transitionDuration, layer);
        }

        public void SetAnimBool(AnimParamSO param, bool value)
            => _animator.SetBool(param.HashValue, value);

        public void SetAnimFloat(AnimParamSO param, float value)
            => _animator.SetFloat(param.HashValue, value);

        public void SetAnimFloat(AnimParamSO param, float value, float dampTime)
            => _animator.SetFloat(param.HashValue, value, dampTime, Time.deltaTime);

        public void SetAnimInt(AnimParamSO param, int value)
            => _animator.SetInteger(param.HashValue, value);

        public void SetAnimTrigger(AnimParamSO param)
            => _animator.SetTrigger(param.HashValue);

        private void EndTrigger(AnimationEvent evt)
        {
            if (IsFromLeavingState(evt))
                return;

            OnAnimationEnd?.Invoke();
        }

        private void AttackTrigger(AnimationEvent evt)
        {
            if (IsFromLeavingState(evt))
                return;

            OnAttackTrigger?.Invoke();
        }

        private void WeaponSwapTrigger()
            => OnWeaponSwapTrigger?.Invoke();

        private void FootstepTrigger()
            => OnFootstepTrigger?.Invoke();

        private bool IsFromLeavingState(AnimationEvent evt)
        {
            if (!_animator.IsInTransition(0))
                return false;

            return evt.animatorStateInfo.fullPathHash != _animator.GetNextAnimatorStateInfo(0).fullPathHash;
        }
    }
}
