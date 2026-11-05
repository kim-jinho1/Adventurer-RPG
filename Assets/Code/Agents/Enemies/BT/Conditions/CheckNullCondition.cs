using System;
using Unity.Behavior;
using UnityEngine;

namespace Code.Agents.Enemies.BT.Conditions
{
    /// <summary>
    /// 블랙보드 값이 비었는지 보는 BT 조건입니다.
    /// </summary>
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(
        name: "Check null",
        story: "[Target] is null",
        category: "Conditions",
        id: "cef94d27e26ae6a68debcbeefbb004c4")]
    public partial class CheckNullCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<GameObject> Target;

        public override bool IsTrue()
        {
            if (Target == null)
                return false;

            return Target.Value == null;
        }
    }
}
