using Ami.BroAudio;
using UnityEngine;

namespace Code.Worlds
{
    /// <summary>
    /// 지역(마을 · 필드 · 던전) 하나를 나타내는 데이터입니다. 지역 씬 하나와 짝을 이룹니다.
    /// 지역을 추가할 때는 씬과 이 에셋만 만들면 됩니다.
    /// </summary>
    [CreateAssetMenu(fileName = "Area", menuName = "SO/World/Area", order = 0)]
    public class AreaSO : ScriptableObject
    {
        [field: Header("Area Setting")]
        [field: SerializeField] public string SceneName { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public bool IsSafeZone { get; private set; }      // 적이 없는 지역. 들어오면 자동 저장한다

        [field: Header("Area Sound")]
        [field: SerializeField] public SoundID Bgm { get; private set; }          // 비우면 배경음을 끈다
        [field: SerializeField] public SoundID Ambience { get; private set; }

#if UNITY_EDITOR
        // 씬 이름을 손으로 적으면 오타가 나기 쉬워서, 씬 에셋을 끌어 넣으면 이름을 채워 준다
        [SerializeField] private UnityEditor.SceneAsset scene;

        private void OnValidate()
        {
            if (scene != null)
                SceneName = scene.name;
        }
#endif
    }
}
