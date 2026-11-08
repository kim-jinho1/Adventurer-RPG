using Code.Agents.FSM;
using Code.Animations;
using Code.Interactions;
using Unity.Profiling;
using UnityEngine;

namespace Code.Agents.Characters.FSM
{
    /// <summary>
    /// 채집 · 제작 같은 작업 모션을 정해진 시간 동안 재생하는 상태입니다. 끝까지 마쳐야 상호작용이 이루어지고, 이동 · 공격 · 구르기 · 피격으로 취소됩니다.
    /// </summary>
    public class CharacterWorkState : AbstractCharacterState, ICanAttackState, ICanDodgeState
    {
        private static readonly ProfilerMarker _workHitMarker = new("CharacterWorkState.HandleWorkHit");

        private readonly IWorkModule _workModule;
        private readonly IWorkToolModule _workToolModule;

        private WorkRequest _work;  // Enter에서 복사해 두어 도중에 새 요청 · ClearWork가 와도 시작한 작업을 끝까지 들고 간다
        private float _endTime;

        public CharacterWorkState(Agent agent, AnimParamSO paramSO) : base(agent, paramSO)
        {
            _workModule = _character.GetModule<IWorkModule>();
            _workToolModule = _character.GetModule<IWorkToolModule>();
            Debug.Assert(_workModule != null, $"[CharacterWorkState] IWorkModule 모듈 없음: {_character.name}", _character);
            Debug.Assert(_workToolModule != null, $"[CharacterWorkState] IWorkToolModule 모듈 없음: {_character.name}", _character);
        }

        public override void Enter()
        {
            base.Enter();
            _work = _workModule.CurrentWork;
            Debug.Assert(_work.IsValid, $"[CharacterWorkState] 작업 요청 없이 들어옴 → IWorkModule.RequestWork로 시작: {_character.name}", _character);

            _mover.StopImmediately();
            _mover.CanManualMovement = false;
            LookAtTarget(_work.Target);

            // 뽑은 검 · 방패가 도구와 겹치지 않게 작업 동안만 숨긴다 (등에 넣은 무기는 그대로)
            if (_weaponModule.IsDrawn)
                _weaponModule.SetVisible(false);

            _workToolModule.Show(_work.WorkType);

            _renderer.SetAnimFloat(_character.InteractionParam, _work.WorkType.BlendValue);
            _renderer.PlayClip(_clipHash, 0, 0f);
            _endTime = Time.time + _work.WorkType.Duration;
            _trigger.OnAttackTrigger += HandleWorkHit;
        }

        public override void Update()
        {
            base.Update();
            // 도중에 움직이면 취소하고, 아이템은 끝까지 마쳤을 때만 준다
            if (_character.CharacterInput.InputDirection.sqrMagnitude > _inputThreshold)
            {
                _character.ChangeState(CharacterStateEnum.IDLE);
                return;
            }

            if (Time.time < _endTime)
                return;

            // 작업 사이 대상이 사라졌을 수 있어 다시 확인한다 (대상이 없는 제작 망치질은 모션만)
            IInteractable interactable = _work.Target;
            if (interactable != null && interactable.CanInteract(_character))
                interactable.Interact(_character);

            _workModule.CompleteWork(_work);
            _character.ChangeState(CharacterStateEnum.IDLE);
        }

        public override void Exit()
        {
            _trigger.OnAttackTrigger -= HandleWorkHit;

            // 취소로 빠져도 지난 대상이 남아 다음 작업에 쓰이지 않게 비운다
            _workModule.ClearWork();
            _workToolModule.HideAll();
            _weaponModule.SetVisible(true);
            _mover.CanManualMovement = true;
            _work = default;
            base.Exit();
        }

        private void LookAtTarget(IInteractable interactable)
        {
            if (interactable is not Component target)
                return;

            Vector3 direction = target.transform.position - _character.transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0f)
                _character.transform.rotation = Quaternion.LookRotation(direction);
        }

        /// <summary>
        /// 도구가 대상에 닿는 순간(클립의 AttackTrigger)에 대상에게 알립니다.
        /// </summary>
        private void HandleWorkHit()
        {
            using (_workHitMarker.Auto())
            {
                if (_work.Target is IWorkHitReceiver receiver)
                    receiver.HandleWorkHit(_character);
            }
        }
    }
}
