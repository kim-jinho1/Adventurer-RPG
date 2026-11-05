using UnityEngine;

namespace Code.Agents.Enemies
{
    /// <summary>
    /// 적이 찾을 BT 블랙보드 변수 이름을 지정하는 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "VariableSO", menuName = "Variables/VariableSO", order = 10)]
    public class VariableSO : ScriptableObject
    {
        [field: SerializeField] public BTVariableEnum VariableName { get; private set; }
    }
}
