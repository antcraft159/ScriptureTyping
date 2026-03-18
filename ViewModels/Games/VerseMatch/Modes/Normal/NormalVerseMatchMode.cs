using ScriptureTyping.ViewModels.Games.VerseMatch.Contracts;

namespace ScriptureTyping.ViewModels.Games.VerseMatch.Modes.Normal
{
    /// <summary>
    /// 목적:
    /// 보통 난이도 정책을 제공한다.
    /// </summary>
    public sealed class NormalVerseMatchMode : IVerseMatchMode
    {
        public string Difficulty => VerseMatchDifficulty.Normal;
        public int PairCount => 4;
        public int PreviewLength => 18;
        public int FakeCardCount => 0;
        public bool UseTimer => false;
        public int TimeLimitSeconds => 0;
    }
}