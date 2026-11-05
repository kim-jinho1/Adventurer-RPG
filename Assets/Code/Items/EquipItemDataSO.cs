using System.Collections.Generic;
using System.Text;
using Code.Agents.Stats;
using Code.Equipments;
using Code.Modules;
using UnityEngine;

namespace Code.Items
{
    /// <summary>
    /// 장착하면 스탯 보너스를 주는 장비 아이템입니다. (무기, 방어구)
    /// </summary>
    [CreateAssetMenu(fileName = "EquipItem", menuName = "SO/Item/Equip item", order = 2)]
    public class EquipItemDataSO : AbstractItemDataSO, IItemAction
    {
        [field: Header("Equip Info")]
        [field: SerializeField] public EquipSlotEnum EquipSlot { get; private set; }
        // 켤 메쉬 번호 (무기는 Sword_00N + Shield_00N, 방어구는 Armor_00N 세트의 해당 부위)
        [field: SerializeField, Min(1)] public int MeshIndex { get; private set; } = 1;
        [SerializeField] private List<StatModifyData> modifyStats = new();

        /// <summary> 장착 시 더해지는 스탯 목록입니다. </summary>
        public IReadOnlyList<StatModifyData> ModifyStats => modifyStats;

        public override ItemTypeEnum ItemType
            => ItemTypeEnum.Equipment;

        public string ActionLabel
            => "장착";

        public bool IsConsumedOnAct
            => false;

        /// <summary>
        /// 스탯 보너스를 적용합니다. 이 SO 인스턴스를 키로 써서 해제할 때 정확히 같은 값만 빠집니다.
        /// </summary>
        public virtual void ApplyStats(IStatModule statModule)
        {
            foreach (StatModifyData modifyStat in modifyStats)
            {
                if (modifyStat.TargetStat == null)
                    continue;

                statModule.AddModifier(modifyStat.TargetStat.AssetIndex, this, modifyStat.ModifyValue);
            }
        }

        /// <summary>
        /// 적용했던 스탯 보너스를 되돌립니다.
        /// </summary>
        public virtual void RemoveStats(IStatModule statModule)
        {
            foreach (StatModifyData modifyStat in modifyStats)
            {
                if (modifyStat.TargetStat == null)
                    continue;

                statModule.RemoveModifier(modifyStat.TargetStat.AssetIndex, this);
            }
        }

        public override string GetDescription()
        {
            StringBuilder builder = new(Description);
            foreach (StatModifyData modifyStat in modifyStats)
            {
                if (modifyStat.TargetStat == null)
                    continue;

                builder.AppendLine();
                builder.Append(modifyStat.TargetStat.DisplayName);
                builder.Append(modifyStat.ModifyValue >= 0f ? " +" : " ");
                builder.Append(modifyStat.ModifyValue);
            }

            return builder.ToString();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            // 장비는 하나씩 장착 · 해제하므로 겹치면 장착 후 되돌려 놓을 칸 계산이 어긋난다
            MaxStack = 1;

            // 모디파이어 키가 이 SO 하나라서, 같은 스탯을 두 줄 쓰면 두 번째 줄은 무시된다
            HashSet<StatSO> seen = new();
            foreach (StatModifyData modifyStat in modifyStats)
            {
                if (modifyStat.TargetStat != null && !seen.Add(modifyStat.TargetStat))
                    Debug.LogWarning($"[EquipItemDataSO] 스탯 {modifyStat.TargetStat.name} 중복: {name}", this);
            }
        }

        public bool CanAct(ModuleOwner user, int slotIndex)
        {
            // 같은 장비를 이미 끼고 있으면 EquipAt이 거절하니 버튼도 막는다
            IEquipmentModule equipmentModule = user.GetModule<IEquipmentModule>();
            return equipmentModule != null && !equipmentModule.IsEquipped(this);
        }

        public bool Act(ModuleOwner user, int slotIndex)
        {
            IEquipmentModule equipmentModule = user.GetModule<IEquipmentModule>();
            return equipmentModule != null && equipmentModule.EquipAt(slotIndex);
        }
    }
}
