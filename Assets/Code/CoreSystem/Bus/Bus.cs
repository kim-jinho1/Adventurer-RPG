using System;
using System.Collections.Generic;

namespace Code.CoreSystem.Bus
{
    /// <summary>
    /// 이벤트 버스입니다. 제네릭 타입 T는 IEvent를 구현해야 합니다.
    /// </summary>
    public static class Bus<T> where T : IEvent
    {
        /// <summary> 이벤트를 받는 함수 델리게이트입니다. </summary>
        public delegate void Event(T args);

        // 최초 사용 시 세션 리셋 대상으로 등록합니다(닫힌 제네릭 타입마다 1회).
        static Bus() => BusRegistry.Register(Clear);

        public static Dictionary<OwnerEnum, Event> OnEvent = new()    // 소유자별 구독 목록
        {
            { OwnerEnum.Invalid, null },
            { OwnerEnum.Player, null },
            { OwnerEnum.Enemy, null },
            { OwnerEnum.Unowned, null }
        };

        /// <summary> 소유자 슬롯에 구독한 함수들에 이벤트를 보냅니다. </summary>
        // OnEvent[owner]를 한 번 읽어 호출하므로, 핸들러가 도중에 구독을 바꿔도(델리게이트 불변)
        // 현재 디스패치는 스냅샷된 목록으로 안전하게 진행됩니다.
        public static void Raise(OwnerEnum owner, T evt)
            => OnEvent[owner]?.Invoke(evt);

        /// <summary> 모든 소유자 슬롯에 구독합니다. </summary>
        public static void RegisterForAll(Event handler)
        {
            foreach (OwnerEnum owner in Enum.GetValues(typeof(OwnerEnum)))
                OnEvent[owner] += handler;
        }

        /// <summary> 모든 소유자 슬롯에서 구독을 해제합니다. </summary>
        public static void UnRegisterForAll(Event handler)
        {
            foreach (OwnerEnum owner in Enum.GetValues(typeof(OwnerEnum)))
                OnEvent[owner] -= handler;
        }

        /// <summary>
        /// 모든 Owner 슬롯의 구독을 해제합니다. 세션 리셋 시 BusRegistry가 호출합니다.
        /// </summary>
        public static void Clear()
        {
            foreach (OwnerEnum owner in Enum.GetValues(typeof(OwnerEnum)))
                OnEvent[owner] = null;
        }
    }
}
