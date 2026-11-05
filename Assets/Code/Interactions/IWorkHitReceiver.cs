using Code.Modules;

namespace Code.Interactions
{
    /// <summary>
    /// 작업 모션에서 도구가 닿는 순간을 받는 대상입니다. (나무 · 광석 등)
    /// </summary>
    public interface IWorkHitReceiver
    {
        /// <summary> 작업 모션이 대상에 닿는 순간 호출됩니다. </summary>
        void HandleWorkHit(ModuleOwner worker);
    }
}
