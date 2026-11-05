using Code.Agents;
using Code.CoreSystem.Bus;
using Code.GameEvents.Worlds;
using Code.Modules;
using UnityEngine;

namespace Code.Worlds
{
    /// <summary>
    /// 이 에이전트(마지막 보스)가 죽으면 게임 클리어를 알립니다. 보스 프리팹에 붙입니다.
    /// </summary>
    public class EndingTrigger : MonoBehaviour, IModule
    {
        [Header("Event")]
        [SerializeField] private OwnerEnum eventOwner = OwnerEnum.Player;

        private Agent _owner;

        public void Initialize(ModuleOwner owner)
        {
            _owner = owner as Agent;
            Debug.Assert(_owner != null, $"[EndingTrigger] Agent에 붙어 있지 않음: {gameObject.name}", this);

            if (_owner != null)
                _owner.OnDeath.AddListener(HandleDeath);
        }

        private void OnDestroy()
        {
            if (_owner != null)
                _owner.OnDeath.RemoveListener(HandleDeath);
        }

        private void HandleDeath()
        {
            Bus<GameClearedEvent>.Raise(eventOwner, new GameClearedEvent());
        }
    }
}
