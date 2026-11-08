using Code.Agents.Characters.Attacks;
using Code.Agents.Stats;
using Code.Animations;
using Code.Combats;
using Code.Modules;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Agents.Characters
{
    /// <summary>
    /// 플레이어 공격 모듈 클래스입니다.
    /// </summary>
    public class CharacterAttackModule : AbstractAttackModule<AbstractCharacterAttack>, IAfterInitModule
    {
        [field: SerializeField] public StatSO AttackSpeedStat { get; private set; }
        [field: SerializeField] public StatSO DamageStat { get; private set; }
        [field: SerializeField] public StatSO CriticalStat { get; private set; }
        [field: SerializeField] public StatSO CriticalDamageStat { get; private set; }
        [field: SerializeField] public AnimParamSO AttackSpeedParam { get; private set; }

        public AbstractCharacterAttack CurrentAttack { get; private set; } = null;    // 지금 진행 중인 공격

        public Character Character { get; private set; }

        private IRenderer _renderer;
        private IStatModule _statModule;

        private float _currentAttackSpeed = 1f;
        private float _currentCritical = 0.05f;
        private float _currentCriticalDamage = 1.2f;

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            Character = owner as Character;
            Debug.Assert(Character != null, $"[CharacterAttackModule] Character에 붙어 있지 않음: {gameObject.name}", this);

            _renderer = Owner.GetModule<IRenderer>();
            _statModule = Owner.GetModule<IStatModule>();
            Debug.Assert(_renderer != null, $"[CharacterAttackModule] IRenderer 모듈 없음: {gameObject.name}", this);
            Debug.Assert(_statModule != null, $"[CharacterAttackModule] IStatModule 모듈 없음: {gameObject.name}", this);

            CollectAttacks();
        }

        protected override void InitializeAttack(AbstractCharacterAttack attack)
            => attack.InitializeAttack(this);

        public void AfterInitialize()
        {
            _currentAttackSpeed = _statModule.SubscribeStat(AttackSpeedStat.AssetIndex, HandleAttackSpeedChange, 1);
            _renderer.SetAnimFloat(AttackSpeedParam, _currentAttackSpeed);
            _currentDamage = _statModule.SubscribeStat(DamageStat.AssetIndex, HandleDamageChange, 1);
            _currentCritical = _statModule.SubscribeStat(CriticalStat.AssetIndex, HandleCriticalChange, 0.05f);
            _currentCriticalDamage = _statModule.SubscribeStat(CriticalDamageStat.AssetIndex, HandleCriticalDamageChange, 1.2f);
        }

        private void OnDestroy()
        {
            if (_statModule != null)
            {
                _statModule.UnSubscribeStat(AttackSpeedStat.AssetIndex, HandleAttackSpeedChange);
                _statModule.UnSubscribeStat(DamageStat.AssetIndex, HandleDamageChange);
                _statModule.UnSubscribeStat(CriticalStat.AssetIndex, HandleCriticalChange);
                _statModule.UnSubscribeStat(CriticalDamageStat.AssetIndex, HandleCriticalDamageChange);
            }
        }

        private void HandleAttackSpeedChange(StatSO stat, float currentValue, float prevValue)
        {
            _currentAttackSpeed = currentValue;
            _renderer.SetAnimFloat(AttackSpeedParam, _currentAttackSpeed);
        }

        private void HandleDamageChange(StatSO stat, float currentValue, float prevValue)
        {
            _currentDamage = currentValue;
        }

        private void HandleCriticalChange(StatSO stat, float currentValue, float prevValue)
        {
            _currentCritical = currentValue;
        }

        private void HandleCriticalDamageChange(StatSO stat, float currentValue, float prevValue)
        {
            _currentCriticalDamage = currentValue;
        }

        public override void Attack(int attackIndex, GameObject target = null)
        {
            if (!_attackDict.TryGetValue(attackIndex, out AbstractCharacterAttack targetAttack))
                return;

            if (CurrentAttack != null && CurrentAttack.IsAttacking)
                CurrentAttack.StopAttack();

            CurrentAttack = targetAttack;
            targetAttack.Attack(target);
            SetLastBattle();
        }

        /// <summary> 치명타 확률을 굴려 맞으면 치명타 배율을 곱한 데미지를 돌려줍니다. </summary>
        public float ApplyCritical(float baseDamage, out bool isCritical)
        {
            isCritical = false;
            if (Random.value < _currentCritical)
            {
                baseDamage *= _currentCriticalDamage;
                isCritical = true;
            }

            return baseDamage;
        }
    }
}
