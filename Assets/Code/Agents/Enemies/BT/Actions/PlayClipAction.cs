using System;
using Code.Animations;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Code.Agents.Enemies.BT.Actions
{
    /// <summary>
    /// 적에게 애니메이션 클립을 재생시키는 BT 액션입니다.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "PlayClip",
        story: "[Enemy] play [Animation]",
        category: "Action",
        id: "c224bb16f14ea14432bac5fed929de86")]
    public partial class PlayClipAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<AnimParamSO> Animation;

        protected override Status OnStart()
        {
            if (Enemy.Value == null || Animation.Value == null || Enemy.Value.Renderer == null)
                return Status.Failure;

            IRenderer renderer = Enemy.Value.Renderer;
            if (renderer.CurrentStateHash != Animation.Value.HashValue)
                renderer.CrossFadeState(Animation.Value.HashValue, 0.15f);
            else
                renderer.PlayClip(Animation.Value.HashValue, -1, 0f);

            return Status.Success;
        }
    }
}
