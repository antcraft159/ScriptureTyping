namespace ScriptureTyping.ViewModels.Games.VerseMatch.Contracts
{
    /// <summary>
    /// 목적:
    /// 난이도별 구절 짝 맞추기 모드가 제공해야 하는 공통 규약이다.
    /// </summary>
    public interface IVerseMatchMode
    {
        string Difficulty { get; }
        int PairCount { get; }
        int PreviewLength { get; }
        int FakeCardCount { get; }
        bool UseTimer { get; }
        int TimeLimitSeconds { get; }
    }
}