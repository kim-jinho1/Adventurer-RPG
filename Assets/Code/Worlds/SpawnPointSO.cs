using UnityEngine;

namespace Code.Worlds
{
    /// <summary>
    /// 지역 씬 안 도착 지점의 이름표 에셋입니다. 에셋 자체가 이름이라 필드가 없습니다.
    /// 포탈과 도착 지점에 같은 에셋을 넣으면 그 자리에 내립니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SpawnPoint", menuName = "SO/World/SpawnPoint", order = 1)]
    public class SpawnPointSO : ScriptableObject
    { }
}
