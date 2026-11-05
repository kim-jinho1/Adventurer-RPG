using Code.Animations;
using Code.Combats;
using UnityEngine;

namespace Code.Agents.Characters.FSM
{
    /// <summary>
    /// 캐릭터가 전투 상태로 진입하는 상태를 나타내는 클래스입니다.
    /// </summary>
    public class CharacterCombatEnterState : AbstractCharacterState
    {
        private IWeaponModule _weaponModule;
        private IAnimatorTrigger _trigger;
        private IAttackModule _attackModule;

        public CharacterCombatEnterState(Agent agent, AnimParamSO paramSO) : base(agent, paramSO)
        {
            _weaponModule = agent.GetModule<IWeaponModule>();
            _trigger = agent.GetModule<IAnimatorTrigger>();
            _attackModule = agent.GetModule<IAttackModule>();

            Debug.Assert(_trigger != null, $"[CharacterCombatEnterState] IAnimatorTrigger 모듈 없음: {_character.name}", _character);
            Debug.Assert(_attackModule != null, $"[CharacterCombatEnterState] IAttackModule 모듈 없음: {_character.name}", _character);
            Debug.Assert(_weaponModule != null, $"[CharacterCombatEnterState] IWeaponModule 모듈 없음: {_character.name}", _character);
        }

        public override void Enter()
        {
            base.Enter();
            _mover.StopImmediately();
            _attackModule.SetLastBattle();

            _trigger.OnWeaponSwapTrigger += HandleWeaponSwap;
            _trigger.OnAnimationEnd += HandleAnimationEnd;
        }

        // 손이 등 뒤 무기를 잡는 순간 손 소켓으로 옮긴다
        private void HandleWeaponSwap()
        {
            _weaponModule.Draw();
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
