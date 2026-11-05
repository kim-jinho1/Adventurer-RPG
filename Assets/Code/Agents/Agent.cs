using Code.Modules;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Code.Agents
{
    /// <summary>
    /// Agent 클래스는 Agent의 기본 동작과 상태를 관리합니다.
    /// </summary>
    public class Agent : ModuleOwner
    {
        public bool IsDead { get; protected set; }

        public AgentSensor Sensor { get; private set; }

        [field: FormerlySerializedAs("OnHit")]
        [field: SerializeField] public UnityEvent OnHit { get; private set; }
        [field: FormerlySerializedAs("OnDeath")]
        [field: SerializeField] public UnityEvent OnDeath { get; private set; }

        protected override void InitializeComponents()
        {
            base.InitializeComponents();
            Sensor = GetModule<AgentSensor>();
        }

        protected virtual void Start()
        { }
    }
}
