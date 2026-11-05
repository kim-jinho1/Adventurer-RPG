using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace Code.Agents.Enemies.BT.Events
{
#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Behavior/Event Channels/StateChannel")]
#endif
    /// <summary>
    /// 코드에서 BT로 적 상태(피격 · 사망)를 알리는 이벤트 채널입니다.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [EventChannelDescription(
        name: "StateChannel",
        message: "Set [CurrentState]",
        category: "Events",
        id: "9a635383c23e410be4fb2ae4e211ba89")]
    public sealed partial class StateChannel : EventChannel<EnemyStateEnum>
    { }
}
