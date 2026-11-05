using Ami.BroAudio;
using Code.Agents;
using Code.Modules;
using Unity.Profiling;
using UnityEngine;

namespace Code.Audios
{
    /// <summary>
    /// 몸에서 나는 소리를 냅니다. 휘두르기 · 피격 · 사망 · 무기 뽑기/넣기 · 발소리를 애니메이션 이벤트와 체력 이벤트에 맞춰 재생합니다.
    /// 비워 둔 SoundID는 재생하지 않으므로 필요한 소리만 채우면 됩니다.
    /// </summary>
    public class AgentSoundModule : MonoBehaviour, IModule, IAfterInitModule
    {
        [Header("Combat")]
        [SerializeField] private SoundID swingSound;        // 클립의 AttackTrigger 순간 (타격 타이밍)
        [SerializeField] private SoundID damagedSound;
        [SerializeField] private SoundID deathSound;
        [SerializeField] private bool isDeathSound2D;      // 플레이어 사망 징글처럼 위치 없이 들려줄 소리

        [Header("Body")]
        [SerializeField] private SoundID weaponSwapSound;
        [SerializeField] private SoundID footstepSound;    // 걷기 클립에 FootstepTrigger 이벤트가 있어야 한다

        private static readonly ProfilerMarker _attackTriggerMarker = new("AgentSoundModule.HandleAttackTrigger");

        private Agent _owner;
        private AgentRenderer _renderer;
        private IHealthModule _healthModule;

        public void Initialize(ModuleOwner owner)
        {
            _owner = owner as Agent;
            _renderer = owner.GetModule<AgentRenderer>();
            _healthModule = owner.GetModule<IHealthModule>();
            Debug.Assert(_owner != null, $"[AgentSoundModule] Agent에 붙어 있지 않음: {gameObject.name}", this);
            Debug.Assert(_renderer != null, $"[AgentSoundModule] AgentRenderer 모듈 없음: {gameObject.name}", this);
        }

        public void AfterInitialize()
        {
            if (_renderer != null)
            {
                _renderer.OnAttackTrigger += HandleAttackTrigger;
                _renderer.OnWeaponSwapTrigger += HandleWeaponSwap;
                _renderer.OnFootstepTrigger += HandleFootstep;
            }

            if (_healthModule != null)
                _healthModule.OnDamaged += HandleDamaged;

            if (_owner != null)
                _owner.OnDeath.AddListener(HandleDeath);
        }

        private void OnDestroy()
        {
            if (_renderer != null)
            {
                _renderer.OnAttackTrigger -= HandleAttackTrigger;
                _renderer.OnWeaponSwapTrigger -= HandleWeaponSwap;
                _renderer.OnFootstepTrigger -= HandleFootstep;
            }

            if (_healthModule != null)
                _healthModule.OnDamaged -= HandleDamaged;

            if (_owner != null)
                _owner.OnDeath.RemoveListener(HandleDeath);
        }

        private void HandleAttackTrigger()
        {
            using (_attackTriggerMarker.Auto())
            {
                PlayAtBody(swingSound);
            }
        }

        private void HandleWeaponSwap()
            => PlayAtBody(weaponSwapSound);

        private void HandleFootstep()
        {
            if (_owner != null && _owner.IsDead)
                return;

            PlayAtBody(footstepSound);
        }

        private void HandleDamaged()
            => PlayAtBody(damagedSound);

        private void HandleDeath()
        {
            if (!deathSound.IsValid())
                return;

            if (isDeathSound2D)
                BroAudio.Play(deathSound);
            else
                PlayAtBody(deathSound);
        }

        private void PlayAtBody(SoundID sound)
        {
            if (sound.IsValid())
                BroAudio.Play(sound, transform.position + Vector3.up);
        }
    }
}
