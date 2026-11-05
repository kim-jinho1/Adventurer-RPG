using UnityEngine;

namespace Code.Agents.Enemies
{
    /// <summary>
    /// 적의 데미지와 감지 · 멈춤 · 놓침 거리를 정하는 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "Enemy Attack config", menuName = "Enemy/Attack config", order = 10)]
    public class EnemyAttackConfigSO : ScriptableObject
    {
        [field: SerializeField] public float Damage { get; private set; }
        [field: SerializeField] public float DetectRange { get; private set; }
        [field: SerializeField] public float StoppingDistance { get; private set; }
        [field: SerializeField] public float LoseRange { get; private set; }
    }
}
