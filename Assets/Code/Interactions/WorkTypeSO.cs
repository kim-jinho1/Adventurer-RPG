using UnityEngine;

namespace Code.Interactions
{
    /// <summary>
    /// 해당 작업의 애니메이션을 담고있는 SO입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "Work Type", menuName = "Work/Work Type", order = 0)]
    public class WorkTypeSO : ScriptableObject
    {
        [field: SerializeField] public float BlendValue { get; private set; }           // 블랜드 값
        [field: SerializeField, Min(0f)] public float Duration { get; private set; }    // 재생 시간
    }
}
