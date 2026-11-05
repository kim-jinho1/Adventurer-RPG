using Code.Combats;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Agents.Characters.Attacks
{
    /// <summary>
    /// 캐릭터 공격 데이터 ScriptableObject입니다. 공통 공격 데이터에 스킬 정보와 콤보 수를 더합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "Character attack data", menuName = "Combat/Character attack data", order = 0)]
    public class CharacterAttackDataSO : AttackDataSO
    {
        [field: Header("Character settings")]
        [field: FormerlySerializedAs("skillSprite")]
        [field: SerializeField] public Sprite SkillSprite { get; private set; }
        [field: FormerlySerializedAs("skillDescription")]
        [field: SerializeField, TextArea] public string SkillDescription { get; private set; }

        [field: Header("Combo")]
        [field: FormerlySerializedAs("comboCount")]
        [field: SerializeField] public int ComboCount { get; private set; } = 3;
    }
}
