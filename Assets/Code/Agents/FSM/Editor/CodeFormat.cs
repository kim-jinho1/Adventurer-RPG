namespace Code.Agents.FSM.Editor
{
    /// <summary>
    /// 상태 열거형 코드를 생성할 때 쓰는 문자열 틀입니다.
    /// </summary>
    public static class CodeFormat
    {
        public static readonly string EnumFormat =    // {0} 열거형 이름, {1} 항목들
            @"
namespace Code.Agents.Characters.FSM
{{
    /// <summary>
    /// 캐릭터 상태를 나타내는 열거형입니다.
    /// </summary>
    public enum {0}
    {{
{1}
    }}
}}
";
    }
}
