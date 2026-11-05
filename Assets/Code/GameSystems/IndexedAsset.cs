using UnityEngine;

namespace Code.GameSystems
{
    /// <summary>
    /// 번호(AssetIndex)로 찾을 수 있는 에셋의 추상 클래스입니다. 저장 파일에서 에셋을 번호로 가리킬 때 씁니다.
    /// </summary>
    public abstract class IndexedAsset : ScriptableObject
    {
        [field: SerializeField] public int AssetIndex { get; set; }
    }
}
