using Code.Combats;
using UnityEngine;

namespace Code.Agents.Characters.Attacks
{
    /// <summary>
    /// 플레이어 공격 기능을 담당하는 추상 클래스입니다.
    /// </summary>
    public abstract class AbstractCharacterAttack : MonoBehaviour, IAttack
    {
        [field: SerializeField] public AttackDataSO AttackData { get; private set; }
        public CharacterAttackDataSO CharacterAttackData { get; private set; }    // AttackData를 캐릭터용으로 바꿔 둔 것

        public bool IsAttacking { get; set; }

        public float NormalizedCooldown
            => Mathf.Clamp01((Time.time - _lastAttackTime) / AttackData.Cooldown);

        protected float _lastAttackTime;
        protected CharacterAttackModule _attackModule;
        protected Character _character;

        public virtual void InitializeAttack(IAttackModule attackModule)
        {
            AttackData = AttackData.Clone() as CharacterAttackDataSO; // 시작하면 클론

            _attackModule = attackModule as CharacterAttackModule;
            Debug.Assert(_attackModule != null, $"[AbstractCharacterAttack] CharacterAttackModule에 붙어 있지 않음: {gameObject.name}", this);

            _character = _attackModule.Owner as Character;
            Debug.Assert(_character != null, $"[AbstractCharacterAttack] Character에 붙어 있지 않음: {gameObject.name}", this);

            CharacterAttackData = AttackData as CharacterAttackDataSO;
            Debug.Assert(CharacterAttackData != null,
                $"[AbstractCharacterAttack] AttackData가 CharacterAttackDataSO 아님: {gameObject.name}", this);
        }

        public abstract bool CanAttack(GameObject target = null);

        public virtual void Attack(GameObject target = null)
        {
            IsAttacking = true;
        }

        public virtual void StopAttack()
        {
            IsAttacking = false;
        }
    }
}
