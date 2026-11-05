using Code.Animations;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Agents.FSM
{
    /// <summary>
    /// 상태 하나의 데이터 ScriptableObject입니다. 상태 이름 · 클래스 이름 · 번호 · 애니메이션 파라미터를 가집니다.
    /// </summary>
    [CreateAssetMenu(fileName = "State data", menuName = "FSM/State data", order = 0)]
    public class StateSO : ScriptableObject
    {
        [field: FormerlySerializedAs("StateName")]
        [field: SerializeField] public string StateName { get; private set; }
        [field: FormerlySerializedAs("ClassName")]
        [field: SerializeField] public string ClassName { get; private set; }
        [field: FormerlySerializedAs("StateIndex")]
        [field: SerializeField] public int StateIndex { get; private set; }
        [field: FormerlySerializedAs("StateParam")]
        [field: SerializeField] public AnimParamSO StateParam { get; private set; }
    }
}
