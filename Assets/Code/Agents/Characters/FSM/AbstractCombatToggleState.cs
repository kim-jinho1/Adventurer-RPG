using Code.Animations;

namespace Code.Agents.Characters.FSM
{
    /// <summary>
    /// 무기를 뽑거나 넣는 상태의 추상 클래스입니다. IsDrawing 값 하나로 뽑기와 넣기를 가릅니다.
    /// </summary>
    public abstract class AbstractCombatToggleState : AbstractOneShotState
    {
        protected abstract bool IsDrawing { get; }     // true면 뽑기, false면 넣기

        protected AbstractCombatToggleState(Agent agent, AnimParamSO paramSO) : base(agent, paramSO)
        { }

        public override void Enter()
        {
            base.Enter();
            _mover.StopImmediately();
            if (IsDrawing)
                _attackModule.SetLastBattle();
            else
                _attackModule.ClearLastBattle();

            _trigger.OnWeaponSwapTrigger += HandleWeaponSwap;
        }

        public override void Exit()
        {
            _trigger.OnWeaponSwapTrigger -= HandleWeaponSwap;
            base.Exit();
        }

        private void HandleWeaponSwap()
        {
            // 손이 무기를 잡거나 꽂는 순간에 손 · 등 소켓으로 옮긴다
            if (IsDrawing)
                _weaponModule.Draw();
            else
                _weaponModule.Sheathe();
        }
    }
}
