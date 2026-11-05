namespace Code.Combats
{
    /// <summary>
    /// 무기 모듈을 나타내는 인터페이스입니다.
    /// </summary>
    public interface IWeaponModule
    {
        /// <summary> 무기를 뽑았는지 여부 </summary>
        bool IsDrawn { get; }

        /// <summary> 무기를 손으로 옮겨 뽑습니다. </summary>
        void Draw();
        /// <summary> 무기를 등으로 옮겨 넣습니다. </summary>
        void Sheathe();
        /// <summary> 무기 메쉬를 잠깐 숨기거나 다시 보여 줍니다. 뽑음 · 넣음 상태는 그대로입니다. (작업 도구를 들 때 등) </summary>
        void SetVisible(bool isVisible);
    }
}
