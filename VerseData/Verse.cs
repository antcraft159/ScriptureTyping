namespace ScriptureTyping.Data
{
    /// <summary>
    /// 목적: 암송 구절 한 개(참조/본문)를 표현한다.
    /// </summary>
    public sealed record Verse(string Ref, string Text);
}
