namespace Code.Items
{
    /// <summary>
    /// 장비를 끼우는 슬롯입니다. 슬롯마다 장비 하나만 장착됩니다. 슬롯별 규칙과 메쉬 부위 이름은 EquipSlotTableSO가 정합니다.
    /// </summary>
    public enum EquipSlotEnum
    {
        Weapon = 0,
        Chest = 1,  // 갑옷. 기존 에셋의 Armor(1) 값을 그대로 이어받는다
        Head = 2,
        Arm = 3,
        Waist = 4,
        Leg = 5,
    }
}
