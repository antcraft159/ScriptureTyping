using ScriptureTyping.ViewModels.Games.VerseMatch.Contracts;

namespace ScriptureTyping.ViewModels.Games.VerseMatch.Modes.VeryHard
{
    /// <summary>
    /// 목적:
    /// 매우 어려움 난이도 정책을 제공한다.
    /// </summary>
    public sealed class VeryHardVerseMatchMode : IVerseMatchMode
    {
        public string Difficulty => VerseMatchDifficulty.VeryHard;
        public int PairCount => 5;
        public int PreviewLength => 11;
        public int FakeCardCount => 5;
        public bool UseTimer => true;
        public int TimeLimitSeconds => 60;
    }
}