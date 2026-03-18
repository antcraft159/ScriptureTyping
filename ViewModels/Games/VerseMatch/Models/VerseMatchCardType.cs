namespace ScriptureTyping.ViewModels.Games.VerseMatch.Models
{
    /// <summary>
    /// 목적:
    /// VerseMatch 카드의 종류를 구분한다.
    ///
    /// 주의:
    /// - 기존 코드 호환을 위해 Content와 VerseText를 같은 값으로 둔다.
    /// </summary>
    public enum VerseMatchCardType
    {
        Reference = 0,
        Content = 1,
        VerseText = 1
    }
}