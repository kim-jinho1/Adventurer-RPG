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
        [field: FormerlySerializedAs("OnHit")]
        [field: SerializeField] public UnityEvent OnHit { get; private set; }
        
        [field: FormerlySerializedAs("OnDeath")]
        [field: SerializeField] public UnityEvent OnDeath { get; private set; }
        
        public AgentSensor Sensor { get; private set; }
        
        public bool IsDead { get; private set; }

        protected override void InitializeComponents()
        {
            base.InitializeComponents();
            Sensor = GetModule<AgentSensor>();
        }

        protected virtual void Start()
        { }

        /// <summary>
        /// 사망 여부를 바꾸고, 죽을 때 OnDeath를 알립니다. HealthModule만 부릅니다.
        /// </summary>
        public void SetDead(bool isDead)
        {
            if (IsDead == isDead)
                return;

            IsDead = isDead;
            if (isDead)
                OnDeath?.Invoke();
        }
    }
}
