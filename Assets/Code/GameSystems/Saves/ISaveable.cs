namespace Code.GameSystems.Saves
{
    /// <summary>
    /// 저장 · 불러오기에 참여하는 대상입니다. 자기 데이터를 JSON 문자열로 내놓고, 같은 문자열로 되돌립니다.
    /// 저장할 것이 늘어도 그 모듈만 이 인터페이스를 구현하면 됩니다.
    /// </summary>
    public interface ISaveable
    {
        /// <summary> 저장 파일에서 이 데이터를 찾는 아이디입니다. </summary>
        SaveIdSO SaveId { get; }

        /// <summary> 저장할 데이터를 JSON 문자열로 만듭니다. </summary>
        string GetSaveData();
        /// <summary> GetSaveData가 만든 문자열로 상태를 되돌립니다. </summary>
        void RestoreData(string data);
    }
}
