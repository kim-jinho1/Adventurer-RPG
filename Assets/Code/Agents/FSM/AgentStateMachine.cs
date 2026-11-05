using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Agents.FSM
{
    /// <summary>
    /// 상태 목록(StateSO)으로 상태 객체를 만들어 두고 전환 · 갱신하는 FSM 클래스입니다.
    /// </summary>
    public class AgentStateMachine
    {
        public AgentState CurrentState { get; private set; }

        private Dictionary<int, AgentState> _stateDict;

        public AgentStateMachine(Agent agent, StateSO[] stateList)
        {
            _stateDict = new Dictionary<int, AgentState>();
            foreach (StateSO stateData in stateList)
            {
                Type type = Type.GetType(stateData.ClassName);

                Debug.Assert(type != null,
                    $"[AgentStateMachine] 상태 클래스 {stateData.ClassName} 찾지 못함 ({stateData.name}): {agent.name}", agent);

                AgentState agentState = Activator.CreateInstance(type, agent, stateData.StateParam) as AgentState;
                _stateDict.Add(stateData.StateIndex, agentState);
            }
        }

        /// <summary> 지금 상태를 나가고 번호에 맞는 상태로 들어갑니다. </summary>
        public void ChangeState(int newStateIndex)
        {
            CurrentState?.Exit();
            AgentState newState = _stateDict.GetValueOrDefault(newStateIndex);

            Debug.Assert(newState != null, $"[AgentStateMachine] 상태 번호 {newStateIndex} 찾지 못함");

            CurrentState = newState;
            CurrentState.Enter();
        }

        /// <summary> 지금 상태의 Update를 호출합니다. </summary>
        public void UpdateMachine()
        {
            CurrentState?.Update();
        }
    }
}
