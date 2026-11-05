namespace Code.Modules
{
    /// <summary>
    /// ModuleOwner에 붙는 모듈 인터페이스입니다.
    /// </summary>
    public interface IModule
    {
        /// <summary> 주인(ModuleOwner)의 Awake에서 호출됩니다. 다른 모듈은 여기서 GetModule로 받아 둡니다. </summary>
        void Initialize(ModuleOwner owner);
    }
}
