using Code.Agents.Characters.Attacks;
using Code.Agents.Characters.FSM;
using Code.Agents.FSM;
using Code.Animations;
using Code.Combats;
using Code.Inputs;
using Code.Interactions;
using Code.Modules;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Agents.Characters
{
    /// <summary>
    /// 캐릭터 에이전트 클래스입니다. 플레이어 클래스의 기반이 되는 클래스입니다.
    /// </summary>
    public class Character : Agent
    {
        [field: Header("Character Input")]
        [field: SerializeField] public CharacterInputSO CharacterInput { get; private set; }
        [field: SerializeField] public float SprintSpeedMultiplier { get; private set; } = 2f;
        [field: SerializeField] public float SprintBlendSpeed { get; private set; } = 1.5f;

        [field: Header("Character Info")]
        [field: SerializeField] public GenderEnum Gender { get; private set; }

        [field: Header("Character Animation")]
        [field: SerializeField] public AnimParamSO MoveXParam { get; private set; }         // 2D 스트레이프 블렌드 X (좌우)
        [field: SerializeField] public AnimParamSO MoveYParam { get; private set; }         // 2D 스트레이프 블렌드 Y (전후)
        [field: SerializeField] public AnimParamSO SpeedParam { get; private set; }         // 1D 블렌드 스피드 (정지, 걷기, 달리기)
        [field: SerializeField] public AnimParamSO DamageIndexParam { get; private set; }   // 피격 블렌드 인덱스 (0 비전투, 1 전투)
        [field: SerializeField] public AnimParamSO InteractionParam { get; private set; }   // 작업 블렌드 인덱스 (WorkIndex)
        [field: SerializeField] public AnimParamSO LocomotionCombatParam { get; private set; }
        [field: SerializeField] public float MoveBlendDampTime { get; private set; } = 0.1f;

        // 회피 값은 애니메이터 ROLL 상태 0.85배속에 맞춘 값이다 (시간은 ÷0.85, 속도는 ×0.85로 구르는 거리 약 4.2m)
        [field: Header("Character Action")]
        [field: SerializeField] public float DodgeInvincibleDuration { get; private set; } = 0.59f;     // 구르기 시작 후 무적 시간
        [field: SerializeField] public float DodgeMoveDuration { get; private set; } = 0.82f;           // 구르기 시작 후 밀리는 시간
        [field: SerializeField] public float DodgeSpeed { get; private set; } = 5.1f;                   // 구르는 속도 (m/s)
        [field: SerializeField] public float HitKnockbackDuration { get; private set; } = 0.2f;         // 맞고 밀리는 시간

        [Header("Character State Machine")]
        [SerializeField] private StateListSO stateList;

        public ActionDataModule ActionDataModule { get; private set; }
        public IHealthModule HealthModule { get; private set; }

        private AgentStateMachine _stateMachine;
        private IAttackModule _attackModule;
        private IWeaponModule _weaponModule;
        private IInteractionModule _interactionModule;
        private IWorkModule _workModule;

        private bool _isAttackRequested;
        private float _lastMaxHealth;

        protected override void InitializeComponents()
        {
            base.InitializeComponents();
            _stateMachine = new AgentStateMachine(this, stateList.States);
            _attackModule = GetModule<IAttackModule>();
            HealthModule = GetModule<IHealthModule>();
            ActionDataModule = GetModule<ActionDataModule>();
            _weaponModule = GetModule<IWeaponModule>();
            _interactionModule = GetModule<IInteractionModule>();
            _workModule = GetModule<IWorkModule>();

            Debug.Assert(_attackModule != null, $"[Character] IAttackModule 모듈 없음: {gameObject.name}", this);
            Debug.Assert(HealthModule != null, $"[Character] IHealthModule 모듈 없음: {gameObject.name}", this);
            Debug.Assert(ActionDataModule != null, $"[Character] ActionDataModule 모듈 없음: {gameObject.name}", this);
            Debug.Assert(_weaponModule != null, $"[Character] IWeaponModule 모듈 없음: {gameObject.name}", this);
            Debug.Assert(_interactionModule != null, $"[Character] IInteractionModule 모듈 없음: {gameObject.name}", this);
            Debug.Assert(_workModule != null, $"[Character] IWorkModule 모듈 없음: {gameObject.name}", this);
        }

        protected override void AfterInitializeComponents()
        {
            base.AfterInitializeComponents();
            OnHit.AddListener(HandleHit);
            OnDeath.AddListener(HandleDead);

            CharacterInput.OnInteractKeyPressed += HandleInteractKey;
            CharacterInput.OnAttackKeyPressed += HandleAttackKey;
            CharacterInput.OnDodgeKeyPressed += HandleDodgeKey;
            CharacterInput.OnCombatToggleKeyPressed += HandleCombatToggleKey;

            HealthModule.OnHealthChange += HandleHealthChange;
            _workModule.OnWorkRequested += HandleWorkRequested;
            _lastMaxHealth = HealthModule.MaxHealth;
        }

        protected override void Start()
        {
            base.Start();
            _stateMachine.ChangeState((int)CharacterStateEnum.IDLE);
        }

        private void Update()
        {
            ProcessAttackRequest();
            _stateMachine.UpdateMachine();
        }

        private void OnDestroy()
        {
            OnHit.RemoveListener(HandleHit);
            OnDeath.RemoveListener(HandleDead);

            CharacterInput.OnInteractKeyPressed -= HandleInteractKey;
            CharacterInput.OnAttackKeyPressed -= HandleAttackKey;
            CharacterInput.OnDodgeKeyPressed -= HandleDodgeKey;
            CharacterInput.OnCombatToggleKeyPressed -= HandleCombatToggleKey;

            HealthModule.OnHealthChange -= HandleHealthChange;
            _workModule.OnWorkRequested -= HandleWorkRequested;
        }

        private void HandleHit()
        {
            // 공격 중에는 약공격에 끊기지 않는다 (체력 · 전투 시간은 HandleHealthChange에서 이미 반영)
            if (_stateMachine.CurrentState is CharacterAttackState && !ActionDataModule.IsLastHitStagger)
                return;

            ChangeState(CharacterStateEnum.HIT);
        }

        private void HandleDead()
        {
            ChangeState(CharacterStateEnum.DEAD);
        }

        /// <summary>
        /// 체력이 줄면 전투 상태를 갱신합니다. 사망 판정은 HealthModule이 합니다.
        /// </summary>
        private void HandleHealthChange(float before, float health, float max)
        {
            if (IsDead)
                return;

            // 약공격(경직 없음)에도 전투 상태를 켜려고 OnHit 대신 체력 감소로 보되, 방어구 교체로 최대 체력이 줄어 함께 내려간 것은 뺀다
            bool isMaxHealthChanged = !Mathf.Approximately(max, _lastMaxHealth);
            _lastMaxHealth = max;
            if (health < before && !isMaxHealthChanged)
                _attackModule.SetLastBattle();
        }

        private void HandleWorkRequested(WorkRequest request)
        {
            if (IsDead || _stateMachine.CurrentState is not ICanWorkState)
            {
                _workModule.ClearWork();
                return;
            }

            ChangeState(CharacterStateEnum.WORK);
        }

        private void HandleInteractKey()
        {
            if (IsDead || _stateMachine.CurrentState is not ICanWorkState)
                return;

            IInteractable interactable = _interactionModule.FindClosestInteractable();
            if (interactable == null)
                return;

            if (interactable is IWorkInteractable work)
            {
                _workModule.RequestWork(work, work.WorkType);
            }
            else
            {
                _interactionModule.InteractImmediately(interactable);
            }
        }

        // 입력 콜백 안에서는 UI가 지난 프레임 상태라 클릭이 UI 위인지 정확히 알 수 없으므로, 요청만 받고 Update에서 처리한다
        private void HandleAttackKey()
        {
            _isAttackRequested = true;
        }

        private void ProcessAttackRequest()
        {
            if (!_isAttackRequested)
                return;

            _isAttackRequested = false;
            // 화면 공격 버튼도 UI라서, 마우스 클릭일 때만 UI 위인지 확인한다
            if (CharacterInput.IsLastAttackFromMouse && IsPointerOverUI())
                return;

            TryAttack();
        }

        private static bool IsPointerOverUI()
            => Mouse.current != null && UIPointerChecker.IsOverUI(Mouse.current.position.ReadValue());

        private void TryAttack()
        {
            if (!_weaponModule.IsDrawn && _stateMachine.CurrentState is ICanAttackState { CanAttack: true })
            {
                ChangeState(CharacterStateEnum.COMBAT_ENTER);
                return;
            }

            if (!_attackModule.CanAttack((int)AttackKeyEnum.BASEKEY))
                return;

            ChangeState(CharacterStateEnum.ATTACK);
            _attackModule.Attack((int)AttackKeyEnum.BASEKEY);
        }

        private void HandleDodgeKey()
        {
            if (IsDead || _stateMachine.CurrentState is not ICanDodgeState)
                return;

            ChangeState(CharacterStateEnum.DODGE);
        }

        private void HandleCombatToggleKey()
        {
            if (IsDead || _stateMachine.CurrentState is not ICanToggleCombatState)
                return;

            ChangeState(_weaponModule.IsDrawn ? CharacterStateEnum.COMBAT_EXIT : CharacterStateEnum.COMBAT_ENTER);
        }

        /// <summary>
        /// 죽은 캐릭터를 체력을 가득 채워 되살리고 Idle로 돌립니다.
        /// </summary>
        public void Revive()
        {
            if (!IsDead)
                return;

            HealthModule.Revive();
            ChangeState(CharacterStateEnum.IDLE);
        }

        /// <summary> 상태를 바꿉니다. </summary>
        public void ChangeState(CharacterStateEnum newState)
        {
            _stateMachine.ChangeState((int)newState);
        }

        /// <summary> 지금 상태를 가져옵니다. </summary>
        public AgentState GetCurrentState()
            => _stateMachine.CurrentState;
    }
}
