using Ami.BroAudio;
using Code.CoreSystem.Bus;
using Code.GameEvents.Worlds;
using Code.Worlds;
using UnityEngine;

namespace Code.Audios
{
    /// <summary>
    /// Core 씬에 두는 지역 소리 관리자입니다. 지역에 들어오면 그 지역(AreaSO)의 배경음 · 환경음으로 바꾸고,
    /// 포탈을 탈 때와 게임을 깼을 때 알림음을 냅니다.
    /// </summary>
    public class AreaSoundPlayer : MonoBehaviour
    {
        [Header("Fade")]
        [SerializeField, Min(0f)] private float fadeTime = 1.5f;

        [Header("Sounds")]
        [SerializeField] private SoundID portalSound;
        [SerializeField] private SoundID gameClearedSound;

        [Header("Event")]
        [SerializeField] private OwnerEnum eventOwner = OwnerEnum.Player;

        private BusSubscriptionScope _busScope;
        private SoundID _currentBgm;
        private SoundID _currentAmbience;

        private void Awake()
        {
            _busScope = new BusSubscriptionScope();
            _busScope.Subscribe<AreaEnteredEvent>(eventOwner, HandleAreaEntered);
            _busScope.Subscribe<AreaTransitionRequestEvent>(eventOwner, HandleTransitionRequest);
            _busScope.Subscribe<GameClearedEvent>(eventOwner, HandleGameCleared);
        }

        private void OnDestroy()
        {
            _busScope?.Dispose();
        }

        private void HandleAreaEntered(AreaEnteredEvent evt)
        {
            if (evt.Area == null)
                return;

            _currentBgm = SwitchLoop(_currentBgm, evt.Area.Bgm);
            _currentAmbience = SwitchLoop(_currentAmbience, evt.Area.Ambience);
        }

        private void HandleTransitionRequest(AreaTransitionRequestEvent evt)
        {
            if (portalSound.IsValid())
                BroAudio.Play(portalSound);
        }

        private void HandleGameCleared(GameClearedEvent evt)
        {
            // 엔딩 징글이 묻히지 않게 배경음을 줄여서 끈다
            _currentBgm = SwitchLoop(_currentBgm, default);
            if (gameClearedSound.IsValid())
                BroAudio.Play(gameClearedSound);
        }

        // 같은 소리가 이미 돌고 있으면 그대로 두고(마을 ↔ 필드가 같은 곡이면 끊기지 않게), 다르면 페이드로 바꾼다
        private SoundID SwitchLoop(SoundID current, SoundID next)
        {
            if (current.IsValid() && current.Equals(next) && BroAudio.HasAnyPlayingInstances(current))
                return current;

            if (current.IsValid())
                BroAudio.Stop(current, fadeTime);

            if (next.IsValid())
                BroAudio.Play(next, fadeTime);

            return next;
        }
    }
}
