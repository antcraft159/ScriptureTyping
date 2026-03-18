using ScriptureTyping.ViewModels.Games.VerseMatch.Contracts;

namespace ScriptureTyping.ViewModels.Games.VerseMatch.Modes.Hard
{
    /// <summary>
    /// 목적:
    /// 어려움 난이도 정책을 제공한다.
    /// </summary>
    public sealed class HardVerseMatchMode : IVerseMatchMode
    {
        public string Difficulty => VerseMatchDifficulty.Hard;
        public int PairCount => 5;
        public int PreviewLength => 14;
        public int FakeCardCount => 3;
        public bool UseTimer => false;
        public int TimeLimitSeconds => 0;
    }
}