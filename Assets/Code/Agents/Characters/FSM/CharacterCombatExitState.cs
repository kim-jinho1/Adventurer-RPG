using Code.Animations;
using Code.Combats;
using UnityEngine;

namespace Code.Agents.Characters.FSM
{
    /// <summary>
    /// 캐릭터가 전투 상태에서 벗어나는 상태를 나타내는 클래스입니다.
    /// </summary>
    public class CharacterCombatExitState : AbstractCharacterState
    {
        private IWeaponModule _weaponModule;
        private IAnimatorTrigger _trigger;
        private IAttackModule _attackModule;

        public CharacterCombatExitState(Agent agent, AnimParamSO paramSO) : base(agent, paramSO)
        {
            _weaponModule = agent.GetModule<IWeaponModule>();
            _trigger = agent.GetModule<IAnimatorTrigger>();
            _attackModule = agent.GetModule<IAttackModule>();

            Debug.Assert(_trigger != null, $"[CharacterCombatExitState] IAnimatorTrigger 모듈 없음: {_character.name}", _character);
            Debug.Assert(_weaponModule != null, $"[CharacterCombatExitState] IWeaponModule 모듈 없음: {_character.name}", _character);
            Debug.Assert(_attackModule != null, $"[CharacterCombatExitState] IAttackModule 모듈 없음: {_character.name}", _character);
        }

        public override void Enter()
        {
            base.Enter();
            _mover.StopImmediately();
            _attackModule.ClearLastBattle();

            _trigger.OnWeaponSwapTrigger += HandleWeaponSwap;
            _trigger.OnAnimationEnd += HandleAnimationEnd;
        }

        // 손이 무기를 등 뒤에 꽂는 순간 등 소켓으로 옮긴다
        private void HandleWeaponSwap()
        {
            _weaponModule.Sheathe();
        }

        public override void HandleAnimationEnd()
        {
            _character.ChangeState(CharacterStateEnum.IDLE);
        }

        public override void Exit()
        {
            _trigger.OnWeaponSwapTrigger -= HandleWeaponSwap;
            _trigger.OnAnimationEnd -= HandleAnimationEnd;
            base.Exit();
        }
    }
}
