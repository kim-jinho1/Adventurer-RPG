namespace Code.Levels
{
    /// <summary>
    /// 경험치와 레벨을 관리하는 모듈 인터페이스입니다.
    /// </summary>
    public interface ILevelModule
    {
        /// <summary> 경험치 변경 델리게이트입니다. </summary>
        delegate void ExpChangeHandler(int currentExp, int requiredExp);
        /// <summary> 레벨업 델리게이트입니다. </summary>
        delegate void LevelUpHandler(int level);

        /// <summary> 경험치가 바뀌면 호출됩니다. 레벨업이 있었다면 레벨업 처리가 끝난 뒤에 한 번 호출됩니다. </summary>
        event ExpChangeHandler OnExpChanged;
        /// <summary> 레벨이 오를 때마다 호출됩니다. 한 번에 여러 레벨이 오르면 여러 번 호출됩니다. </summary>
        event LevelUpHandler OnLevelUp;

        /// <summary> 현재 레벨입니다. </summary>
        int Level { get; }
        /// <summary> 현재 레벨에서 모은 경험치입니다. </summary>
        int CurrentExp { get; }
        /// <summary> 다음 레벨까지 필요한 경험치입니다. 최대 레벨이면 0입니다. </summary>
        int RequiredExp { get; }
        /// <summary> 최대 레벨인지 확인합니다. </summary>
        bool IsMaxLevel { get; }

        /// <summary> 경험치를 더하고, 필요한 만큼 레벨업합니다. </summary>
        void AddExp(int amount);
    }
}
