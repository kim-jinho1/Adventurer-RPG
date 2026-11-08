using Code.Combats;
using Code.Modules;
using UnityEngine;

namespace Code.Agents.Enemies
{
    /// <summary>
    /// 적 공격 모듈입니다. 자식에 붙은 공격을 번호로 모아 시전하고 전투 상태를 관리합니다.
    /// </summary>
    public class EnemyAttackModule : AbstractAttackModule<AbstractEnemyAttack>
    {
        public AbstractEnemy OwnerEnemy { get; private set; }

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            OwnerEnemy = owner as AbstractEnemy;
            Debug.Assert(OwnerEnemy != null, $"[EnemyAttackModule] AbstractEnemy에 붙어 있지 않음: {gameObject.name}", this);

            CollectAttacks();

            _currentDamage = OwnerEnemy.AttackConfig.Damage;
        }

        protected override void InitializeAttack(AbstractEnemyAttack attack)
            => attack.InitializeAttack(this);

        public override void Attack(int attackIndex, GameObject target = null)
        {
            if (_attackDict.TryGetValue(attackIndex, out AbstractEnemyAttack targetAttack))
            {
                OwnerEnemy.PlayAnimation(targetAttack.AttackData.AnimatorParam);
                targetAttack.Attack(target);
            }
        }

        /// <summary> 모든 공격의 쿨타임을 초기화합니다. </summary>
        public void ResetAllCooldowns()
        {
            foreach (AbstractEnemyAttack attack in _attackDict.Values)
                attack.ResetCooldown();
        }

        // 적은 공격이 끝날 때 전투 시간을 다시 센다(캐릭터는 시작할 때).
        public override void InvokeAttackEnd()
        {
            SetLastBattle();
            base.InvokeAttackEnd();
        }
    }
}
