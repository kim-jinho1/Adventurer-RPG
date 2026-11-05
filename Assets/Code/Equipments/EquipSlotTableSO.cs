using System;
using System.Collections.Generic;
using Code.Items;
using UnityEngine;

namespace Code.Equipments
{
    /// <summary>
    /// 장비 슬롯마다 비울 수 있는지, 비었을 때 어떤 메쉬를 보여 줄지, 메쉬 부위 이름이 무엇인지 모아 둔 표입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "EquipSlotTable", menuName = "SO/Item/Equip slot table", order = 12)]
    public class EquipSlotTableSO : ScriptableObject
    {
        [SerializeField] private List<EquipSlotRule> rules = new();

        public IReadOnlyList<EquipSlotRule> Rules => rules;

        private Dictionary<EquipSlotEnum, EquipSlotRule> _ruleDict;

        private void OnEnable()
        {
            BuildTable();
        }

        private void BuildTable()
        {
            _ruleDict = new Dictionary<EquipSlotEnum, EquipSlotRule>(rules.Count);
            foreach (EquipSlotRule slotRule in rules)
                _ruleDict.TryAdd(slotRule.Slot, slotRule); // 중복이면 먼저 등록된 것을 쓴다 (경고는 OnValidate)
        }

        /// <summary> 슬롯의 규칙을 찾습니다. 표에 없으면 false를 반환합니다. </summary>
        public bool TryGetRule(EquipSlotEnum slot, out EquipSlotRule rule)
        {
            if (_ruleDict == null)
                BuildTable();

            return _ruleDict.TryGetValue(slot, out rule);
        }

        /// <summary> 비울 수 없는(교체만 되는) 슬롯인지 확인합니다. 표에 없는 슬롯은 비울 수 있습니다. </summary>
        public bool IsRequired(EquipSlotEnum slot)
            => TryGetRule(slot, out EquipSlotRule rule) && rule.IsRequired;

        /// <summary> 슬롯이 비었을 때 켤 메쉬 번호입니다. 0이면 아무것도 켜지 않습니다. </summary>
        public int GetEmptyMeshIndex(EquipSlotEnum slot)
            => TryGetRule(slot, out EquipSlotRule rule) ? rule.EmptyMeshIndex : 0;

        private void OnValidate()
        {
            HashSet<EquipSlotEnum> slots = new();
            foreach (EquipSlotRule rule in rules)
                if (!slots.Add(rule.Slot))
                    Debug.LogWarning($"[EquipSlotTableSO] 슬롯 {rule.Slot} 중복: {name}", this);

            foreach (EquipSlotEnum slot in Enum.GetValues(typeof(EquipSlotEnum)))
                if (!slots.Contains(slot))
                    Debug.LogWarning($"[EquipSlotTableSO] 슬롯 {slot} 규칙 없음: {name}", this);

            BuildTable();
        }
    }
}