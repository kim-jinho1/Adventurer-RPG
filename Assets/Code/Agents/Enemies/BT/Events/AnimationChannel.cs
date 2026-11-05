using System;
using Code.Animations;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace Code.Agents.Enemies.BT.Events
{
#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Behavior/Event Channels/AnimationChannel")]
#endif
    /// <summary>
    /// 코드에서 BT로 애니메이션 재생을 요청하는 이벤트 채널입니다.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [EventChannelDescription(
        name: "AnimationChannel",
        message: "Set Animation to [Clip]",
        category: "Events",
        id: "cc91419fc2f566462575cd3c3067147c")]
    public sealed partial class AnimationChannel : EventChannel<AnimParamSO>
    { }
}
