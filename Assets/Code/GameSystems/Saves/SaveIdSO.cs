using UnityEngine;

namespace Code.GameSystems.Saves
{
    /// <summary>
    /// 저장 데이터 한 덩어리를 구분하는 아이디입니다. 저장하는 모듈마다 하나씩 만들어 연결합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SaveId", menuName = "SO/System/Save id", order = 0)]
    public class SaveIdSO : ScriptableObject
    {
        [field: SerializeField] public int Id { get; private set; }
        [SerializeField, TextArea] private string description;
    }
}
