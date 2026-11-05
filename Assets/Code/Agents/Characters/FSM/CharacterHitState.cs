using Code.Agents.FSM;
using Code.Animations;
using Code.Combats;
using UnityEngine;

namespace Code.Agents.Characters.FSM
{
    /// <summary>
    /// 캐릭터의 Hit 상태를 나타내는 클래스입니다.
    /// </summary>
    public class CharacterHitState : AbstractCharacterState, ICanAttackState
    {
        private const float _knockbackDuration = 0.2f;

        // 약공격에 맞은 Hit는 공격으로 끊을 수 있고, 강공격(경직)은 끝날 때까지 못 끊는다
        public bool CanAttack
            => !_character.ActionDataModule.IsLastHitStagger;

        private IAnimatorTrigger _trigger;
        private IAttackModule _attackModule;
        private IWeaponModule _weaponModule;

        private float _knockbackEndTime;
        private bool _isKnockbackActive;

        public CharacterHitState(Agent agent, AnimParamSO paramSO) : base(agent, paramSO)
        {
            _trigger = agent.GetModule<IAnimatorTrigger>();
            _attackModule = agent.GetModule<IAttackModule>();
            _weaponModule = agent.GetModule<IWeaponModule>();
            Debug.Assert(_trigger != null, $"[CharacterHitState] IAnimatorTrigger 모듈 없음: {_character.name}", _character);
            Debug.Assert(_attackModule != null, $"[CharacterHitState] IAttackModule 모듈 없음: {_character.name}", _character);
            Debug.Assert(_weaponModule != null, $"[CharacterHitState] IWeaponModule 모듈 없음: {_character.name}", _character);
        }

        public override void Enter()
        {
            base.Enter();
            _renderer.SetAnimFloat(_character.DamageIndexParam, _attackModule.IsInBattle ? 1f : 0f);
            _renderer.PlayClip(_clipHash, 0, 0f);

            _mover.IsRotationLocked = true;
            _mover.StopImmediately();
            _mover.CanManualMovement = false;

            Vector3 lookRotation = -_character.ActionDataModule.LastHitDirection;
            lookRotation.y = 0f;
            if (lookRotation.sqrMagnitude > 0f)
                _character.transform.rotation = Quaternion.LookRotation(lookRotation);

            _mover.SetAutoMovement(_character.ActionDataModule.LastKnockbackForce);
            _knockbackEndTime = Time.time + _knockbackDuration;
            _isKnockbackActive = true;

            _trigger.OnAnimationEnd += HandleAnimationEnd;
        }

        public override void Update()
        {
            base.Update();
            if (Time.time >= _knockbackEndTime && _isKnockbackActive)
            {
                _mover.StopImmediately();
                _isKnockbackActive = false;
            }
        }

        public override void HandleAnimationEnd()
        {
            if (!_weaponModule.IsDrawn && _attackModule.IsInBattle)
                _character.ChangeState(CharacterStateEnum.COMBAT_ENTER);
            else
                _character.ChangeState(CharacterStateEnum.IDLE);
        }

        public override void Exit()
        {
            _mover.CanManualMovement = true;
            _mover.IsRotationLocked = false;

            _trigger.OnAnimationEnd -= HandleAnimationEnd;
            base.Exit();
        }
    }
}
