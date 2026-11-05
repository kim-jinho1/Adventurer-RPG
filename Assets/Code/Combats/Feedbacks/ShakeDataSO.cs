using UnityEngine;

namespace Code.Combats.Feedbacks
{
    /// <summary>
    /// 카메라 셰이크 한 번의 세기와 길이입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "Shake", menuName = "Feedback/Shake data", order = 0)]
    public class ShakeDataSO : ScriptableObject
    {
        [field: SerializeField, Min(0f)] public float Force { get; private set; } = 0.3f;
        [field: SerializeField, Min(0.01f)] public float Duration { get; private set; } = 0.15f;
    }
}
