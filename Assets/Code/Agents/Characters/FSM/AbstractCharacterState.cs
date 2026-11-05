using Code.Agents.FSM;
using Code.Animations;
using UnityEngine;

namespace Code.Agents.Characters.FSM
{
    /// <summary>
    /// 캐릭터 상태의 추상 클래스입니다.
    /// </summary>
    public abstract class AbstractCharacterState : AgentState
    {
        protected IMover _mover;
        protected Character _character;

        protected const float _inputThreshold = 0.01f;     // sqrMagnitude와 비교하므로 입력 길이 0.1의 제곱

        public AbstractCharacterState(Agent agent, AnimParamSO paramSO) : base(agent, paramSO)
        {
            _character = agent as Character;
            Debug.Assert(_character != null, $"[AbstractCharacterState] Character에 붙어 있지 않음: {agent.name}", agent);
            _mover = agent.GetModule<IMover>();
        }

        /// <summary>
        /// 무기를 들고 있으면 Locomotion_Combat, 아니면 이 상태의 기본 Locomotion을 재생합니다.
        /// </summary>
        protected void PlayLocomotion(bool isWeaponDrawn)
        {
            _renderer.CrossFadeState(isWeaponDrawn ? _character.LocomotionCombatParam.HashValue : _clipHash);
        }
    }
}