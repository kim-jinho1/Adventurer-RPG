using Code.Items;

namespace Code.Inventories
{
    /// <summary>
    /// 월드에서 IItemCollector에 줍기를 요청하는 쪽입니다. 받을지는 줍는 쪽이 정합니다.
    /// </summary>
    public interface IPickupable
    {
        /// <summary> 주워 달라는 아이템입니다. 줍는 쪽이 받을지 정할 때 씁니다. </summary>
        AbstractItemDataSO Item { get; }

        /// <summary> 줍는 쪽이 받아 주기로 했을 때 부릅니다. 줍는 쪽으로 날아가기 시작합니다. </summary>
        void StartFlying();
    }
}
