using Code.Animations;

namespace Code.Agents.FSM
{
    /// <summary>
    /// AgentState 클래스는 Agent의 상태를 나타내며, 상태 전환 및 애니메이션 재생을 관리합니다.
    /// </summary>
    public class AgentState
    {
        protected Agent _agent;

        protected int _clipHash;

        protected IRenderer _renderer;

        public AgentState(Agent agent, AnimParamSO paramSO)
        {
            _agent = agent;
            _clipHash = paramSO != null ? paramSO.HashValue : 0;
            _renderer = agent.GetModule<IRenderer>();
        }

        /// <summary>
        /// 상태의 업데이트 로직을 처리합니다. 이 메서드는 매 프레임 호출됩니다.
        /// </summary>
        public virtual void Update()
        { }

        /// <summary>
        /// 상태에 진입할 때 호출되며, 해당 상태의 애니메이션을 재생합니다.
        /// </summary>
        public virtual void Enter()
        {
            _renderer.CrossFadeState(_clipHash);
        }

        /// <summary>
        /// 상태에서 벗어날 때 호출되며, 필요한 정리 작업을 수행합니다.
        /// </summary>
        public virtual void Exit()
        { }

        /// <summary>
        /// 애니메이션이 끝나면 호출됩니다. 끝을 받아야 하는 상태가 override합니다.
        /// </summary>
        public virtual void HandleAnimationEnd()
        { }
    }
}
