namespace Code.Items.Drops
{
    /// <summary>
    /// 바닥에 떨어진 아이템(ItemPickup)의 상태입니다.
    /// </summary>
    public enum PickupStateEnum
    {
        Launching, // 방금 튕겨나온 상태, 아직 줍기 불가능
        Resting, // 바닥에 있고, 줍는 쪽에서 요청 하지 않은 상태
        Requested, // 줍는 쪽에서 요청하고 답을 기다리는중. 대기 줄에 포함됨
        Flying // 줍는 쪽이 받는중 날아가는 상태
    }
}
