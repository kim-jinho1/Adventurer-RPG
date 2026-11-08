using Code.Animations;
using UnityEngine;

namespace Code.Combats
{
    /// <summary>
    /// 캐릭터·적 공격의 공통 부모입니다. 공격 데이터, 쿨타임, 애니메이션 트리거 구독을 가집니다.
    /// TModule로 자기 공격 모듈 타입을 정해 두어, 모듈을 캐스팅 없이 받습니다.
    /// </summary>
    public abstract class AbstractAttack<TModule> : MonoBehaviour, IAttack
        where TModule : class, IAttackModule
    {
        [field: SerializeField] public AttackDataSO AttackData { get; private set; }

        public float NormalizedCooldown
            => Mathf.Clamp01((Time.time - _lastAttackTime) / AttackData.Cooldown);

        protected TModule _attackModule;
        protected IAnimatorTrigger _trigger;

        protected float _lastAttackTime;

        /// <summary> 공격 모듈과 애니메이션 트리거를 받아 둡니다. 모듈의 Initialize에서 호출됩니다. </summary>
        public virtual void InitializeAttack(TModule attackModule)
        {
            _attackModule = attackModule;

            _trigger = attackModule.Owner.GetModule<IAnimatorTrigger>();
            Debug.Assert(_trigger != null, $"[AbstractAttack] IAnimatorTrigger 모듈 없음: {gameObject.name}", this);
        }

        public abstract bool CanAttack(GameObject target = null);

        public abstract void Attack(GameObject target = null);

        public virtual void StopAttack()
        { }

        /// <summary> 쿨타임을 지금부터 다시 셉니다. </summary>
        public void ResetCooldown()
            => _lastAttackTime = Time.time;

        /// <summary> 타격 타이밍(AttackTrigger)과 클립 끝(EndTrigger) 이벤트를 구독합니다. </summary>
        protected void SubscribeTriggers()
        {
            UnSubscribeTriggers();
            _trigger.OnAttackTrigger += HandleCastDamage;
            _trigger.OnAnimationEnd += HandleAnimationEnd;
        }

        /// <summary> 트리거 이벤트 구독을 해제합니다. </summary>
        protected void UnSubscribeTriggers()
        {
            _trigger.OnAttackTrigger -= HandleCastDamage;
            _trigger.OnAnimationEnd -= HandleAnimationEnd;
        }

        /// <summary> 타격 타이밍에 호출됩니다. </summary>
        protected virtual void HandleCastDamage()
        { }

        /// <summary> 공격 클립이 끝나면 호출됩니다. </summary>
        protected virtual void HandleAnimationEnd()
        { }
    }
}
