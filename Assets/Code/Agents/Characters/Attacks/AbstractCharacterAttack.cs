using Code.Combats;
using UnityEngine;

namespace Code.Agents.Characters.Attacks
{
    /// <summary>
    /// 플레이어 공격 기능을 담당하는 추상 클래스입니다.
    /// </summary>
    public abstract class AbstractCharacterAttack : AbstractAttack<CharacterAttackModule>
    {
        public CharacterAttackDataSO CharacterAttackData { get; private set; }    // AttackData를 캐릭터용으로 바꿔 둔 것

        public bool IsAttacking { get; set; }

        protected Character _character;

        public override void InitializeAttack(CharacterAttackModule attackModule)
        {
            base.InitializeAttack(attackModule);
            _character = attackModule.Character;

            CharacterAttackData = AttackData as CharacterAttackDataSO;
            Debug.Assert(CharacterAttackData != null,
                $"[AbstractCharacterAttack] AttackData가 CharacterAttackDataSO 아님: {gameObject.name}", this);
        }

        public override void Attack(GameObject target = null)
        {
            IsAttacking = true;
        }

        public override void StopAttack()
        {
            IsAttacking = false;
        }
    }
}
