using Ami.BroAudio;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Audios
{
    /// <summary>
    /// 버튼을 누르면 클릭음을 냅니다. 버튼 오브젝트에 같이 붙입니다.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonSound : MonoBehaviour
    {
        [SerializeField] private SoundID clickSound;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleClicked);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(HandleClicked);
        }

        private void HandleClicked()
        {
            if (clickSound.IsValid())
                BroAudio.Play(clickSound);
        }
    }
}
