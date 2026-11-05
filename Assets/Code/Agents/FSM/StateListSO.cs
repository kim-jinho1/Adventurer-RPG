using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Agents.FSM
{
    /// <summary>
    /// 한 FSM이 쓰는 상태 목록 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "Fsm state manager", menuName = "FSM/State list", order = 5)]
    public class StateListSO : ScriptableObject
    {
        [field: FormerlySerializedAs("enumName")]
        [field: SerializeField] public string EnumName { get; private set; }
        [field: FormerlySerializedAs("states")]
        [field: SerializeField] public StateSO[] States { get; private set; }
    }
}
