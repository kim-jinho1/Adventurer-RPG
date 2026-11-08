using Code.Agents.FSM;
using Code.Combats;
using Unity.Profiling;
using UnityEngine;

namespace Code.Agents.Characters.Attacks
{
    /// <summary>
    /// 콤보 공격을 처리하는 클래스입니다. 플레이어의 공격 모듈과 상호작용하며, 콤보 공격의 애니메이션, 이동, 데미지 캐스팅을 관리합니다.
    /// </summary>
    public class ComboAttack : AbstractCharacterAttack
    {
        [Header("Combo Attack Settings")]
        [SerializeField] private float comboWindow = 0.8f;
        [SerializeField] private int comboCounter;

        [Header("Combo Attack Movements")]
        [SerializeField] private Vector3[] comboMovements;
        [SerializeField] private Vector3[] casterOffsets;
        [SerializeField] private Vector3[] overrideKnockBackForce;
        [SerializeField] private bool[] comboStagger;               // 콤보별 경직 여부 (막타만 true)

        private static readonly ProfilerMarker _castDamageMarker = new("ComboAttack.HandleCastDamage");

        private IRenderer _renderer;
        private IMover _mover;
        private AbstractDamageCaster _damageCaster;

        public override void InitializeAttack(CharacterAttackModule attackModule)
        {
            base.InitializeAttack(attackModule);
            _renderer = _character.GetModule<IRenderer>();
            _mover = _character.GetModule<IMover>();

            Debug.Assert(_renderer != null, $"[ComboAttack] IRenderer 모듈 없음: {gameObject.name}", this);
            Debug.Assert(_mover != null, $"[ComboAttack] IMover 모듈 없음: {gameObject.name}", this);

            _damageCaster = GetComponentInChildren<AbstractDamageCaster>();
            Debug.Assert(_damageCaster != null, $"[ComboAttack] AbstractDamageCaster 컴포넌트 없음: {gameObject.name}", this);

            _damageCaster.InitCaster(_character);
        }

        public override bool CanAttack(GameObject target = null)
        {
            return _character.GetCurrentState() is ICanAttackState { CanAttack: true };
        }

        public override void Attack(GameObject target = null)
        {
            base.Attack(target);
            if (comboCounter >= CharacterAttackData.ComboCount || Time.time >= _lastAttackTime + comboWindow)
                comboCounter = 0;

            _renderer.PlayAttack(comboCounter);

            Vector3 movement = comboCounter < comboMovements.Length ? comboMovements[comboCounter] : Vector3.zero;
            Vector3 offset = comboCounter < casterOffsets.Length ? casterOffsets[comboCounter] : Vector3.zero;

            _damageCaster.transform.localPosition = offset;
            _mover.SetAutoMovement(_character.transform.TransformDirection(movement));

            SubscribeTriggers();
        }

        public override void StopAttack()
        {
            UnSubscribeTriggers();
            base.StopAttack();
        }

        protected override void HandleCastDamage()
        {
            using (_castDamageMarker.Auto())
            {
                _trigger.OnAttackTrigger -= HandleCastDamage;

                float damage = _attackModule.GetBaseDamage(AttackData);
                Vector3 kbPower = comboCounter < overrideKnockBackForce.Length
                    ? overrideKnockBackForce[comboCounter]
                    : AttackData.KnockBackForce;
                bool causesStagger = comboCounter < comboStagger.Length && comboStagger[comboCounter];

                float finalDamage = _attackModule.ApplyCritical(damage, out bool isCritical);
                _damageCaster.CastDamage(finalDamage, kbPower, isCritical, causesStagger);

                _mover.StopImmediately();
            }
        }

        protected override void HandleAnimationEnd()
        {
            UnSubscribeTriggers();
            ++comboCounter;
            _lastAttackTime = Time.time;
            IsAttacking = false;
            _mover.StopImmediately();
            _attackModule.InvokeAttackEnd();
        }
    }
}
