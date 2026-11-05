namespace Code.Agents.Stats
{
    /// <summary>
    /// 스탯 모듈 인터페이스입니다.
    /// </summary>
    public interface IStatModule
    {
        /// <summary> 모든 스탯을 가져옵니다 </summary>
        StatSO[] GetAllStats();
        /// <summary> 특정 인덱스의 스탯을 가져옵니다 </summary>
        StatSO GetStat(int statIndex);
        /// <summary> 특정 인덱스의 스탯을 가져옵니다. 존재하지 않으면 false를 반환합니다. </summary>
        bool TryGetStat(int statIndex, out StatSO outStat);

        /// <summary> 특정 스탯에 모디파이어를 추가합니다. key는 모디파이어를 식별하기 위한 고유한 값이어야 합니다. </summary>
        void AddModifier(int statIndex, object key, float value);
        /// <summary> 특정 스탯에서 모디파이어를 제거합니다. key는 모디파이어를 식별하기 위한 고유한 값이어야 합니다. </summary>
        void RemoveModifier(int statIndex, object key);

        /// <summary> 특정 스탯의 값을 구독합니다. </summary>
        float SubscribeStat(int statIndex, StatSO.ValueChangeHandler handler, float defaultValue);
        /// <summary> 특정 스탯의 구독을 해제합니다. </summary>
        void UnSubscribeStat(int statIndex, StatSO.ValueChangeHandler handler);
    }
}
