using System.Collections.Generic;
using System.Linq;
using Code.Modules;
using UnityEngine;

namespace Code.Agents.Stats
{
    /// <summary>
    /// StatModule는 StatSO를 관리하는 모듈입니다.
    /// </summary>
    public class StatModule : MonoBehaviour, IModule, IStatModule
    {
        [SerializeField] private StatOverride[] statOverrides;

        private ModuleOwner _owner;
        private Dictionary<int, StatSO> _stats;

        public void Initialize(ModuleOwner owner)
        {
            _owner = owner;
            _stats = statOverrides.ToDictionary(stat => stat.StatData.AssetIndex, stat => stat.CreateStat());
        }

        public StatSO[] GetAllStats()
            => _stats.Values.ToArray();

        public StatSO GetStat(int statIndex)
            => _stats.GetValueOrDefault(statIndex);

        public bool TryGetStat(int statIndex, out StatSO outStat)
            => _stats.TryGetValue(statIndex, out outStat);

        public void AddModifier(int statIndex, object key, float value)
        {
            if (_stats.TryGetValue(statIndex, out StatSO stat))
            {
                stat.AddModifier(key, value);
                return;
            }

            Debug.LogWarning($"[StatModule] 스탯 찾지 못함 (index {statIndex}) → 모디파이어 추가 안 함: {gameObject.name}", this);
        }

        public void RemoveModifier(int statIndex, object key)
        {
            if (_stats.TryGetValue(statIndex, out StatSO stat))
            {
                stat.RemoveModifier(key);
                return;
            }

            Debug.LogWarning($"[StatModule] 스탯 찾지 못함 (index {statIndex}) → 모디파이어 제거 안 함: {gameObject.name}", this);
        }

        public float SubscribeStat(int statIndex, StatSO.ValueChangeHandler handler, float defaultValue)
        {
            if (!_stats.TryGetValue(statIndex, out StatSO targetStat))
                return defaultValue;

            targetStat.OnValueChanged += handler;
            return targetStat.Value;
        }

        public void UnSubscribeStat(int statIndex, StatSO.ValueChangeHandler handler)
        {
            if (_stats.TryGetValue(statIndex, out StatSO targetStat))
                targetStat.OnValueChanged -= handler;
        }
    }
}
