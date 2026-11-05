using System;
using Code.Items;
using UnityEngine;

namespace Code.UIs
{
    /// <summary>
    /// 가방 창의 장비 칸 하나의 설정입니다. 어느 장비 슬롯을 보여 줄지와 칸 뷰를 묶습니다.
    /// </summary>
    [Serializable]
    public class EquipSlotViewData
    {
        [field: SerializeField] public EquipSlotEnum Slot { get; private set; }
        [field: SerializeField] public ItemSlotView View { get; private set; }
    }
}
