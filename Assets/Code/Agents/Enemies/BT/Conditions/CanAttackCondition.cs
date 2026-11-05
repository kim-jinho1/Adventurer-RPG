using System;
using Unity.Behavior;
using UnityEngine;

namespace Code.Agents.Enemies.BT.Conditions
{
    /// <summary>
    /// 지정한 번호의 공격을 지금 시전할 수 있는지 보는 BT 조건입니다.
    /// </summary>
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(
        name: "CanAttack",
        story: "[Enemy] can attack: [Number] to [Target]",
        category: "Conditions",
        id: "c1bd3e657f5284f189243e93d24ad658")]
    public partial class CanAttackCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<int> Number;
        [SerializeReference] public BlackboardVariable<GameObject> Target;

        public override bool IsTrue()
        {
            if(Enemy.Value == null || Enemy.Value.AttackModule == null)
                return false;

            return Enemy.Value.AttackModule.CanAttack(Number.Value, Target.Value);
        }
    }
}
