using System.Collections.Generic;
using Code.Agents.Enemies.BT.Events;
using Code.Animations;
using Code.Combats;
using Code.Modules;
using Unity.Behavior;
using Unity.Behavior.GraphFramework;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Agents.Enemies
{
    /// <summary>
    /// AbstractEnemy 클래스는 적 캐릭터의 기본 기능을 정의하는 추상 클래스입니다.
    /// </summary>
    public abstract class AbstractEnemy : Agent
    {
        [field: SerializeField] public EnemyAttackConfigSO AttackConfig { get; private set; }
        [SerializeField] private VariableSO[] btVariables;
        [FormerlySerializedAs("resetCooldownOnHit")]
        [SerializeField] private bool isCooldownResetOnHit = true;    // 보스는 꺼서 맞아도 공격 쿨타임이 유지되게 한다

        public BehaviorGraphAgent GraphAgent { get; private set; }

        public IMover Mover { get; private set; }
        public IRenderer Renderer { get; private set; }
        public IAttackModule AttackModule { get; private set; }
        public IHealthModule HealthModule { get; private set; }

        public ActionDataModule ActionDataModule { get; private set; }

        public Vector3 HomePosition { get; private set; }    // 처음 자리 (타깃을 놓치면 돌아갈 곳)

        private BlackboardVariable<StateChannel> _stateChannel;
        private BlackboardVariable<AnimationChannel> _animationChannel;

        private Dictionary<BTVariableEnum, SerializableGUID> _variablesDict;
        private const string _deadBodyLayerName = "DeadBody";

        protected override void InitializeComponents()
        {
            base.InitializeComponents();
            GraphAgent = GetComponent<BehaviorGraphAgent>();

            Debug.Assert(GraphAgent != null, $"[AbstractEnemy] BehaviorGraphAgent 컴포넌트 없음: {gameObject.name}", this);

            Mover = GetModule<IMover>();
            Renderer = GetModule<IRenderer>();
            HealthModule = GetModule<IHealthModule>();
            ActionDataModule = GetModule<ActionDataModule>();
            AttackModule = GetModule<IAttackModule>();

            Debug.Assert(Mover != null, $"[AbstractEnemy] IMover 모듈 없음: {gameObject.name}", this);
            Debug.Assert(Renderer != null, $"[AbstractEnemy] IRenderer 모듈 없음: {gameObject.name}", this);
            Debug.Assert(HealthModule != null, $"[AbstractEnemy] IHealthModule 모듈 없음: {gameObject.name}", this);
            Debug.Assert(ActionDataModule != null, $"[AbstractEnemy] ActionDataModule 모듈 없음: {gameObject.name}", this);

            _variablesDict = new Dictionary<BTVariableEnum, SerializableGUID>();

            if (GraphAgent != null)
            {
                foreach (VariableSO variable in btVariables)
                {
                    Debug.Assert(variable != null, $"[AbstractEnemy] btVariables에 빈 칸 있음: {gameObject.name}", this);
                    if(variable == null)
                        continue;

                    if (!GraphAgent.GetVariableID(variable.VariableName.ToString(), out var guid))
                    {
                        Debug.LogError($"[AbstractEnemy] BT 변수 {variable.VariableName} 찾지 못함: {gameObject.name}", this);
                        continue;
                    }

                    _variablesDict.Add(variable.VariableName, guid);
                }
            }

            SetVariableValue(BTVariableEnum.Enemy, this);
        }

        protected override void AfterInitializeComponents()
        {
            base.AfterInitializeComponents();
            HealthModule.OnHealthChange += HandleHealthChange;
            OnHit.AddListener(HandleHit);
        }

        protected override void Start()
        {
            base.Start();
            if (!GetVariable(BTVariableEnum.StateChannel, out _stateChannel))
                Debug.LogError($"[AbstractEnemy] BT 변수 StateChannel 찾지 못함: {gameObject.name}", this);

            if (!GetVariable(BTVariableEnum.AnimationChannel, out _animationChannel))
                Debug.LogError($"[AbstractEnemy] BT 변수 AnimationChannel 찾지 못함: {gameObject.name}", this);

            HomePosition = transform.position;
        }

        private void OnDestroy()
        {
            HealthModule.OnHealthChange -= HandleHealthChange;
            OnHit.RemoveListener(HandleHit);
        }

        /// <summary>
        /// 체력이 0이 되면 사망 상태를 보냅니다. 피격(HIT)은 경직 있는 공격일 때만 HandleHit에서 보냅니다.
        /// </summary>
        private void HandleHealthChange(float before, float health, float max)
        {
            if (IsDead || health >= before)
                return;

            if (health <= 0)
            {
                IsDead = true;
                if (TryGetChannel(BTVariableEnum.StateChannel, ref _stateChannel))
                    _stateChannel.Value.SendEventMessage(EnemyStateEnum.DEAD);

                // 보스의 EndingTrigger 같은 바깥 구독자가 받도록 플레이어처럼 사망을 알린다
                OnDeath?.Invoke();
            }

            if (isCooldownResetOnHit && AttackModule is EnemyAttackModule enemyAttackModule)
                enemyAttackModule.ResetAllCooldowns();
        }

        /// <summary>
        /// 경직 있는 공격에 맞으면 행동 트리에 피격 상태를 보냅니다.
        /// </summary>
        private void HandleHit()
        {
            if (TryGetChannel(BTVariableEnum.StateChannel, ref _stateChannel))
                _stateChannel.Value.SendEventMessage(EnemyStateEnum.HIT);
        }

        /// <summary>
        /// BTVariableEnum에 해당하는 변수의 값을 설정합니다.
        /// </summary>
        public void SetVariableValue<T>(BTVariableEnum variableName, T value)
        {
            if (GraphAgent == null || _variablesDict == null)
                return;

            if (_variablesDict.TryGetValue(variableName, out SerializableGUID guid))
            {
                GraphAgent.SetVariableValue(guid, value);
                return;
            }

            Debug.LogError($"[AbstractEnemy] BT 변수 {variableName} 찾지 못함: {gameObject.name}", this);
        }

        /// <summary>
        /// BTVariableEnum에 해당하는 변수를 가져옵니다.
        /// </summary>
        public bool GetVariable<T>(BTVariableEnum variableName, out BlackboardVariable<T> variable)
        {
            if (GraphAgent == null || _variablesDict == null)
            {
                variable = null;
                return false;
            }

            if (_variablesDict.TryGetValue(variableName, out SerializableGUID guid))
                return GraphAgent.GetVariable(guid, out variable);

            Debug.LogError($"[AbstractEnemy] BT 변수 {variableName} 찾지 못함: {gameObject.name}", this);
            variable = null;
            return false;
        }

        /// <summary>
        /// 적 캐릭터를 사망 상태로 설정하고, 관련 처리를 수행합니다.
        /// </summary>
        public void SetDead()
        {
            if (GraphAgent != null)
                GraphAgent.enabled = false;

            IsDead = true;
            gameObject.layer = LayerMask.NameToLayer(_deadBodyLayerName);
            Destroy(gameObject, 3f); // 사망 모션이 끝나도록 3초 뒤 지운다 (사라지는 연출은 아직 없음)
        }

        /// <summary> BT 애니메이션 채널로 클립 재생을 요청합니다. </summary>
        public void PlayAnimation(AnimParamSO param)
        {
            if (TryGetChannel(BTVariableEnum.AnimationChannel, ref _animationChannel))
                _animationChannel.Value.SendEventMessage(param);
        }

        /// <summary>
        /// 높이(y)를 뺀 XZ 평면 거리의 제곱을 반환합니다. 추적·정지·공격 사거리 판정이 같은 기준을 쓰도록 합니다.
        /// </summary>
        public float GetFlatSqrDistance(Vector3 targetPosition)
        {
            Vector3 offset = targetPosition - transform.position;
            offset.y = 0f;
            return offset.sqrMagnitude;
        }

        private bool TryGetChannel<T>(BTVariableEnum variableName, ref BlackboardVariable<T> channel)
        {
            return channel != null || GetVariable(variableName, out channel);
        }
    }
}
