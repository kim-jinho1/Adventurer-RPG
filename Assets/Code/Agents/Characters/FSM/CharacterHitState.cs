using Code.Agents.FSM;
using Code.Animations;
using UnityEngine;

namespace Code.Agents.Characters.FSM
{
    /// <summary>
    /// 캐릭터의 Hit 상태를 나타내는 클래스입니다.
    /// </summary>
    public class CharacterHitState : AbstractOneShotState, ICanAttackState
    {
        // 약공격에 맞은 Hit는 공격으로 끊을 수 있고, 강공격(경직)은 끝날 때까지 못 끊는다
        public bool CanAttack
            => !_character.ActionDataModule.IsLastHitStagger;

        private float _knockbackEndTime;
        private bool _isKnockbackActive;

        public CharacterHitState(Agent agent, AnimParamSO paramSO) : base(agent, paramSO)
        { }

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
            _knockbackEndTime = Time.time + _character.HitKnockbackDuration;
            _isKnockbackActive = true;
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
            if (!TryEnterCombat())
                _character.ChangeState(CharacterStateEnum.IDLE);
        }

        public override void Exit()
        {
            _mover.CanManualMovement = true;
            _mover.IsRotationLocked = false;
            base.Exit();
        }
    }
}
