using System;

namespace Code.Animations
{
    /// <summary>
    /// 애니메이터 트리거 이벤트를 정의하는 인터페이스입니다.
    /// </summary>
    public interface IAnimatorTrigger
    {
        event Action OnAnimationEnd;                // 애니메이션 종료 이벤트
        event Action OnAttackTrigger;               // 공격 트리거 이벤트
        event Action OnWeaponSwapTrigger;           // 무기 뽑기/넣기 클립에서 손이 등 소켓에 닿는 순간
    }
}
