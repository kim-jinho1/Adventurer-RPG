using UnityEngine;

namespace Code.Animations
{
    /// <summary>
    /// 애니메이터 파라미터를 ScriptableObject로 정의하는 클래스입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "Anim param", menuName = "System/Animator Param", order = 0)]
    public class AnimParamSO : ScriptableObject
    {
        [field: SerializeField] public string ParamName { get; private set; }   // 애니메이터 파라미터 이름
        [field: SerializeField] public int HashValue { get; private set; }      // 애니메이터 파라미터의 해시 값

        private void OnValidate()
        {
            HashValue = Animator.StringToHash(ParamName);
        }
    }
}
