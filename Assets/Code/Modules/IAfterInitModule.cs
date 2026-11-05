namespace Code.Modules
{
    /// <summary>
    /// 모든 모듈의 Initialize가 끝난 뒤 한 번 더 초기화가 필요한 모듈 인터페이스입니다.
    /// </summary>
    public interface IAfterInitModule
    {
        /// <summary> 모든 모듈의 Initialize가 끝난 뒤 호출됩니다. 다른 모듈의 값이 필요한 초기화는 여기서 합니다. </summary>
        void AfterInitialize();
    }
}
