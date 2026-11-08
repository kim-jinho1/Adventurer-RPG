using Code.Combats;

namespace Code.Agents.Enemies
{
    /// <summary>
    /// 적 공격의 추상 클래스입니다. 공통 부분은 AbstractAttack이 가지고, 실제 공격 방식은 자식 클래스가 정합니다.
    /// </summary>
    public abstract class AbstractEnemyAttack : AbstractAttack<EnemyAttackModule>
    {
        protected AbstractEnemy _enemy;

        public override void InitializeAttack(EnemyAttackModule attackModule)
        {
            base.InitializeAttack(attackModule);
            _enemy = attackModule.OwnerEnemy;
        }
    }
}
