using System;
using Code.Items;
using UnityEngine;

namespace Code.Equipments
{
    /// <summary>
    /// 장비 슬롯 하나의 규칙입니다. EquipSlotTableSO에 슬롯마다 한 줄씩 둡니다.
    /// </summary>
    [Serializable]
    public struct EquipSlotRule
    {
        [field: SerializeField] public EquipSlotEnum Slot { get; private set; }
        [field: SerializeField] public bool IsRequired { get; private set; }         // 켜면 비울 수 없고 교체만 된다 (무기)
        [field: SerializeField] public int EmptyMeshIndex { get; private set; }      // 비었을 때 켤 메쉬 번호. 0이면 아무것도 켜지 않는다
        [field: SerializeField] public string MeshPartSuffix { get; private set; }   // 방어구 세트 안에서 이 부위를 찾는 이름 끝 (_Head). 무기는 비운다
    }
}
