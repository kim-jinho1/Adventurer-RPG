using System;
using Code.Agents.Stats;
using Code.CoreSystem.Bus;
using Code.GameSystems.Saves;
using Code.Modules;
using UnityEngine;

namespace Code.Levels
{
    /// <summary>
    /// 경험치를 모아 레벨업하고, 레벨업마다 기본 스탯(BaseValue)을 올립니다. 버스의 ExpGainEvent도 받아 처리합니다.
    /// </summary>
    public class LevelModule : MonoBehaviour, IModule, IAfterInitModule, ILevelModule, ISaveable
    {
        /// <summary>
        /// 저장 파일에 쓰는 레벨 · 경험치 데이터입니다.
        /// </summary>
        [Serializable]
        private struct LevelSaveData
        {
            public int Level;
            public int CurrentExp;
        }

        [Header("Level Setting")]
        [SerializeField] private LevelTableSO levelTable;
        [SerializeField] private OwnerEnum expEventOwner = OwnerEnum.Player;

        [field: Header("Save")]
        [field: SerializeField] public SaveIdSO SaveId { get; private set; }

        public event ILevelModule.ExpChangeHandler OnExpChanged;
        public event ILevelModule.LevelUpHandler OnLevelUp;

        public int Level { get; private set; } = 1;
        public int CurrentExp { get; private set; }

        public int RequiredExp
            => levelTable != null ? levelTable.GetRequiredExp(Level) : 0;

        public bool IsMaxLevel
            => levelTable == null || levelTable.IsMaxLevel(Level);

        private IStatModule _statModule;
        private BusSubscriptionScope _busScope;

        public void Initialize(ModuleOwner owner)
        {
            _statModule = owner.GetModule<IStatModule>();
            Debug.Assert(_statModule != null, $"[LevelModule] IStatModule 모듈 없음: {gameObject.name}", this);
            Debug.Assert(levelTable != null, $"[LevelModule] levelTable 연결 안 됨: {gameObject.name}", this);
        }

        public void AfterInitialize()
        {
            _busScope = new BusSubscriptionScope();
            _busScope.Subscribe<ExpGainEvent>(expEventOwner, HandleExpGain);
        }

        private void OnDestroy()
        {
            _busScope?.Dispose();
        }

        public void AddExp(int amount)
        {
            if (amount <= 0 || IsMaxLevel)
                return;

            CurrentExp += amount;
            while (TryLevelUp())
            { }

            // 최대 레벨에 닿으면 남은 경험치는 쓸 곳이 없어 버린다
            if (IsMaxLevel)
                CurrentExp = 0;

            OnExpChanged?.Invoke(CurrentExp, RequiredExp);
        }

        private bool TryLevelUp()
        {
            if (IsMaxLevel || CurrentExp < RequiredExp)
                return false;

            CurrentExp -= RequiredExp;
            Level++;
            ApplyLevelUpStats(1);
            OnLevelUp?.Invoke(Level);
            return true;
        }

        public string GetSaveData()
            => JsonUtility.ToJson(new LevelSaveData { Level = Level, CurrentExp = CurrentExp });

        public void RestoreData(string data)
        {
            LevelSaveData saveData = JsonUtility.FromJson<LevelSaveData>(data);
            int targetLevel = levelTable != null ? Mathf.Clamp(saveData.Level, 1, levelTable.MaxLevel) : 1;

            // 레벨업 스탯은 BaseValue에 누적되므로, 지금 레벨과의 차이만큼만 더하거나 뺀다
            ApplyLevelUpStats(targetLevel - Level);
            Level = targetLevel;
            CurrentExp = IsMaxLevel ? 0 : Mathf.Clamp(saveData.CurrentExp, 0, Mathf.Max(RequiredExp - 1, 0));

            // 불러오기는 레벨업이 아니라서 OnLevelUp(토스트)은 부르지 않고 표시만 갱신한다
            OnExpChanged?.Invoke(CurrentExp, RequiredExp);
        }

        /// <summary>
        /// 레벨업 증가량 × levelCount를 기본값에 더합니다. 장비처럼 뺄 일이 없어 모디파이어가 아닌 BaseValue를 올린다.
        /// </summary>
        private void ApplyLevelUpStats(int levelCount)
        {
            if (levelCount == 0 || _statModule == null)
                return;

            foreach (StatModifyData levelUpStat in levelTable.LevelUpStats)
            {
                if (levelUpStat.TargetStat != null && _statModule.TryGetStat(levelUpStat.TargetStat.AssetIndex, out StatSO stat))
                    stat.BaseValue += levelUpStat.ModifyValue * levelCount;
            }
        }

        private void HandleExpGain(ExpGainEvent evt)
        {
            AddExp(evt.Amount);
        }
    }
}
