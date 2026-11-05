using System;
using Code.Combats;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Code.Agents.Enemies.BT.Actions
{
    /// <summary>
    /// 지정한 번호의 공격을 시전하고 공격이 끝날 때까지 기다리는 BT 액션입니다.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "EnemyAttack",
        story: "[Enemy] Attack: [Number] to [Target]",
        category: "Action",
        id: "9de062e50b9c2eb7699e1c1b5f7aad43")]
    public partial class EnemyAttackAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<int> Number;
        [SerializeReference] public BlackboardVariable<GameObject> Target;

        private const float _maxAttackDuration = 5f; // 끝 이벤트가 안 와도 이 시간이 지나면 실패 처리

        private IAttackModule _attackModule;
        private bool _isAttackEnd;
        private float _startTime;

        protected override Status OnStart()
        {
            if (Enemy.Value == null || Enemy.Value.AttackModule == null)
                return Status.Failure;

            _attackModule = Enemy.Value.AttackModule;
            _attackModule.OnAttackEnd += HandleAttackEnd;
            _isAttackEnd = false;
            _startTime = Time.time;
            _attackModule.Attack(Number.Value, Target.Value);
            return Status.Running;
        }

        private void HandleAttackEnd()
        {
            _isAttackEnd = true;
        }

        protected override Status OnUpdate()
        {
            if (_isAttackEnd)
                return Status.Success;

            return Time.time - _startTime > _maxAttackDuration ? Status.Failure : Status.Running;
        }

        protected override void OnEnd()
        {
            // OnStart가 실패해도 OnEnd는 불리므로 구독한 경우에만 정리한다
            if (_attackModule == null)
                return;

            _attackModule.OnAttackEnd -= HandleAttackEnd;

            // 피격 등으로 끝나기 전에 끊겼다면 애니 이벤트 구독이 남지 않도록 공격을 멈춘다
            if (!_isAttackEnd)
                _attackModule.StopAttack(Number.Value);

            _attackModule = null;
            _isAttackEnd = false;
        }
    }
}
