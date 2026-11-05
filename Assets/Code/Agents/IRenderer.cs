using Code.Animations;

namespace Code.Agents
{
    /// <summary>
    /// IRenderer 인터페이스는 Agent의 렌더링 및 애니메이션 관련 기능을 정의합니다.
    /// </summary>
    public interface IRenderer
    {
        /// <summary> 지금 재생 중인 애니메이터 상태의 해시 </summary>
        int CurrentStateHash { get; }

        /// <summary> 애니메이터의 클립을 재생합니다. </summary>
        void PlayClip(int clipHash, int layer = -1, float normalPosition = float.NegativeInfinity);

        /// <summary> 콤보 인덱스에 해당하는 공격 클립을 재생합니다. </summary>
        void PlayAttack(int comboIndex);

        /// <summary> 베이스 레이어의 상태로 크로스페이드합니다. 블렌드 시간은 기본값을 사용합니다. </summary>
        void CrossFadeState(int stateHash);
        /// <summary> 상태로 크로스페이드합니다. 블렌드 시간(초)과 레이어를 직접 지정합니다. </summary>
        void CrossFadeState(int stateHash, float transitionDuration, int layer = 0);

        /// <summary> 애니메이터의 Bool 파라미터를 설정합니다. </summary>
        void SetAnimBool(AnimParamSO param, bool value);
        /// <summary> 애니메이터의 Float 파라미터를 설정합니다. </summary>
        void SetAnimFloat(AnimParamSO param, float value);
        /// <summary> 애니메이터의 Float 파라미터를 설정하며, 감쇠 시간을 적용합니다. </summary>
        void SetAnimFloat(AnimParamSO param, float value, float dampTime);
        /// <summary> 애니메이터의 Int 파라미터를 설정합니다. </summary>
        void SetAnimInt(AnimParamSO param, int value);
        /// <summary> 애니메이터의 Trigger 파라미터를 설정합니다. </summary>
        void SetAnimTrigger(AnimParamSO param);
    }
}
