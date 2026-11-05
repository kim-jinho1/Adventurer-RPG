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

        private ModuleOwner _owner;
        private IHealthModule _healthModule;
        private bool _isDropped;

        public void Initialize(ModuleOwner owner)
        {
            _owner = owner;
            _healthModule = owner.GetModule<IHealthModule>();
            Debug.Assert(_healthModule != null, $"[DropModule] IHealthModule 모듈 없음: {gameObject.name}", this);
            Debug.Assert(dropTable != null, $"[DropModule] dropTable 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(itemScatter != null, $"[DropModule] itemScatter 연결 안 됨: {gameObject.name}", this);
        }

        public void AfterInitialize()
        {
            // 적은 사망 시 Agent.OnDeath를 부르지 않고 BT로 상태만 보내므로, 체력 0을 직접 본다
            if (_healthModule != null)
                _healthModule.OnHealthChange += HandleHealthChange;
        }

        private void OnDestroy()
        {
            if (_healthModule != null)
                _healthModule.OnHealthChange -= HandleHealthChange;
        }

        private void HandleHealthChange(float before, float health, float max)
        {
            if (_isDropped || health > 0f)
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
