using Code.Items;
using UnityEngine;

namespace Code.Inventories
{
    /// <summary>
    /// 월드에서 아이템을 받아 가방에 넣는 쪽입니다. 줍기 · 채집이 이 인터페이스로 아이템을 건넵니다.
    /// </summary>
    public interface IItemCollector
    {
        /// <summary> 떨어진 아이템이 날아올 위치입니다. </summary>
        Transform CollectPoint { get; }

        /// <summary> 아이템을 받습니다. 가방이 차서 못 받은 개수를 반환합니다. </summary>
        int Collect(AbstractItemDataSO item, int count);

        /// <summary> 줍기를 요청받습니다. 받을 수 있으면 바로 날아오게 하고, 못 받으면 대기 줄에 세웁니다. </summary>
        void RequestPickup(IPickupable pickup);
        /// <summary> 범위를 벗어났거나 사라진 아이템을 대기 줄에서 뺍니다. </summary>
        void CancelPickup(IPickupable pickup);
    }
}
