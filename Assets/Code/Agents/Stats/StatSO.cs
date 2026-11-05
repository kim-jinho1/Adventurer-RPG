using System;
using System.Collections.Generic;
using Code.GameSystems;
using UnityEngine;

namespace Code.Agents.Stats
{
    /// <summary>
    /// StatSO는 게임 내에서 사용되는 스탯 데이터를 정의하는 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "stat", menuName = "Stat/Stat data", order = 0)]
    public class StatSO : IndexedAsset, ICloneable
    {
        [field: Header("Stat Info")]
        [field: SerializeField] public string StatName { get; private set; }
        [field: SerializeField] public Sprite StatIcon { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public bool IsPercent { get; private set; }
        [field: SerializeField] public float UpgradeStep { get; private set; } = 1;

        [Header("Stat Value Range")]
        [SerializeField] private float baseValue;
        [SerializeField] private float minValue, maxValue;

        [Header("Stat Description")]
        [SerializeField] private string description;

        public delegate void ValueChangeHandler(StatSO stat, float currentValue, float prevValue);  // Stat의 값이 변경될 때 호출되는 이벤트 핸들러

        public event ValueChangeHandler OnValueChanged;                                             // Stat의 값이 변경될 때 호출되는 이벤트

        public float MaxValue => maxValue;    // 값의 상한
        public float MinValue => minValue;    // 값의 하한

        // 장비 등 모디파이어 합계를 더한 최종 값으로, 기본값과 같은 min · max로 자른다
        public float Value
            => Mathf.Clamp(baseValue + _modifiedValue, MinValue, MaxValue);

        public bool IsMax
            => Mathf.Approximately(Value, maxValue);

        public bool IsMin
            => Mathf.Approximately(Value, minValue);

        private Dictionary<object, float> _modifyValueByKey = new();
        private float _modifiedValue;

        public float BaseValue
        {
            get => baseValue;
            set
            {
                float prevValue = Value;
                baseValue = Mathf.Clamp(value, MinValue, MaxValue);
                TryInvokeValueChangedEvent(Value, prevValue);
            }
        }

        /// <summary> Stat에 Modifier를 추가합니다. 이미 동일한 key가 존재하면 추가되지 않습니다. </summary>
        public void AddModifier(object key, float value)
        {
            if (_modifyValueByKey.ContainsKey(key))
                return;

            float prevValue = Value;

            _modifiedValue += value;
            _modifyValueByKey.Add(key, value);

            TryInvokeValueChangedEvent(Value, prevValue);
        }

        /// <summary> Stat에서 Modifier를 제거합니다. </summary>
        public void RemoveModifier(object key)
        {
            if (!_modifyValueByKey.TryGetValue(key, out float value))
                return;

            float prevValue = Value;

            _modifiedValue -= value;
            _modifyValueByKey.Remove(key);

            TryInvokeValueChangedEvent(Value, prevValue);
        }

        /// <summary> 모든 Modifier를 제거합니다. </summary>
        public void ClearModifier()
        {
            float prevValue = Value;
            _modifyValueByKey.Clear();
            _modifiedValue = 0;
            TryInvokeValueChangedEvent(Value, prevValue);
        }

        /// <summary> SO의 복사본을 생성합니다. </summary>
        public virtual object Clone()
        {
            return Instantiate(this);
        }

        /// <summary> 값이 변경되었을 때 이벤트를 호출합니다. </summary>
        public void TryInvokeValueChangedEvent(float currentValue, float prevValue)
        {
            if (!Mathf.Approximately(currentValue, prevValue)) // 이전 값과 다를 때만
                OnValueChanged?.Invoke(this, currentValue, prevValue);
        }
    }
}
