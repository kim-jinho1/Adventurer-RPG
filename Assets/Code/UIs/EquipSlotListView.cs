using System.Collections.Generic;
using Code.Equipments;
using Code.Items;
using UnityEngine;

namespace Code.UIs
{
    /// <summary>
    /// 가방 창의 장비 칸 목록입니다. 슬롯마다 장착한 장비를 그리고, 칸을 누르면 어느 슬롯을 골랐는지 알립니다.
    /// </summary>
    public class EquipSlotListView : MonoBehaviour
    {
        [Header("Equip Slots")]
        [SerializeField] private List<EquipSlotViewData> equipSlots = new();

        /// <summary> 장비 칸 선택 델리게이트입니다. </summary>
        public delegate void EquipSlotSelectHandler(EquipSlotEnum slot);
        /// <summary> 장비 칸을 누르면 호출됩니다. 무엇을 할지는 이 뷰를 쓰는 창이 정합니다. </summary>
        public event EquipSlotSelectHandler OnEquipSlotSelected;

        private void Awake()
        {
            foreach (EquipSlotViewData equipSlot in equipSlots)
            {
                if (equipSlot.View != null)
                    equipSlot.View.OnSlotClicked += HandleSlotClicked;
            }
        }

        private void OnDestroy()
        {
            foreach (EquipSlotViewData equipSlot in equipSlots)
            {
                if (equipSlot.View != null)
                    equipSlot.View.OnSlotClicked -= HandleSlotClicked;
            }
        }

        /// <summary>
        /// 슬롯마다 장착한 장비를 그립니다. hasSelection이 true면 selectedSlot 칸을 강조합니다.
        /// </summary>
        public void Show(IEquipmentModule equipmentModule, bool hasSelection, EquipSlotEnum selectedSlot)
        {
            foreach (EquipSlotViewData equipSlot in equipSlots)
            {
                if (equipSlot.View == null)
                    continue;

                EquipItemDataSO item = equipmentModule?.GetEquippedItem(equipSlot.Slot);
                equipSlot.View.Show(item, string.Empty);
                equipSlot.View.SetSelected(hasSelection && selectedSlot == equipSlot.Slot);
            }
        }

        private void HandleSlotClicked(ItemSlotView slotView)
        {
            EquipSlotViewData equipSlot = equipSlots.Find(data => data.View == slotView);
            if (equipSlot != null)
                OnEquipSlotSelected?.Invoke(equipSlot.Slot);
        }
    }
}
