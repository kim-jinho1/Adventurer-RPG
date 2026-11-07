using System.Collections.Generic;
using Code.Agents;
using Code.CoreSystem.Bus;
using Code.Levels;
using Code.Modules;
using UnityEngine;

namespace Code.Items.Drops
{
    /// <summary>
    /// 체력이 0이 되면 드랍 테이블대로 아이템을 흩뿌리고 경험치를 버스로 보냅니다. 적에게 붙이는 모듈입니다.
    /// </summary>
    public class DropModule : MonoBehaviour, IModule, IAfterInitModule
    {
        [Header("Drop Setting")]
        [SerializeField] private DropTableSO dropTable;
        [SerializeField] private OwnerEnum expEventOwner = OwnerEnum.Player;
        [SerializeField] private ItemScatterSO itemScatter;

        private readonly List<ItemAmount> _dropResults = new();

        private Agent _agent;
        private ModuleOwner _owner;
        private bool _isDropped;

        public void Initialize(ModuleOwner owner)
        {
            _owner = owner;
            _agent = owner as Agent;
            
            Debug.Assert(_agent != null, $"[DropModule] Agent 컴포넌트 없음: {gameObject.name}", this);
            Debug.Assert(dropTable != null, $"[DropModule] dropTable 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(itemScatter != null, $"[DropModule] itemScatter 연결 안 됨: {gameObject.name}", this);
        }

        public void AfterInitialize()
        {
            if (_agent != null)
                _agent.OnDeath.AddListener(HandleDeath);
        }

        private void OnDestroy()
        {
            if (_agent != null)
                _agent.OnDeath.RemoveListener(HandleDeath);
        }

        private void HandleDeath()
        {
            if (_isDropped)
                return;

            _isDropped = true;
            Drop();
        }

        private void Drop()
        {
            if (dropTable == null)
                return;

            if (dropTable.Exp > 0)
                Bus<ExpGainEvent>.Raise(expEventOwner, new ExpGainEvent(dropTable.Exp));

            if (itemScatter == null)
                return;

            dropTable.Roll(_dropResults);
            foreach (ItemAmount drop in _dropResults)
                itemScatter.Spawn(_owner.transform.position, drop.Item, drop.Count);
        }
    }
}
