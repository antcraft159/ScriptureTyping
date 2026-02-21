namespace ScriptureTyping.ViewModels
{
    /// <summary>
    /// 목적: 한 구절에 대한 타이핑 결과(정답/입력/정오답/오타개수)를 보관한다.
    /// </summary>
    public class TypingResultItem
    {
        public string Ref { get; set; } = string.Empty;
        public string Expected { get; set; } = string.Empty;
        public string Typed { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }
        public int MistakeCount { get; set; }
    }
}
