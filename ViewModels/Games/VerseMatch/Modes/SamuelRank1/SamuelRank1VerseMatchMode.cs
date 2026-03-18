using ScriptureTyping.ViewModels.Games.VerseMatch.Contracts;

namespace ScriptureTyping.ViewModels.Games.VerseMatch.Modes.SamuelRank1
{
    /// <summary>
    /// 목적:
    /// 사무엘 1등 난이도 정책을 제공한다.
    /// </summary>
    public sealed class SamuelRank1VerseMatchMode : IVerseMatchMode
    {
        public string Difficulty => VerseMatchDifficulty.SamuelRank1;
        public int PairCount => 6;
        public int PreviewLength => 10;
        public int FakeCardCount => 7;
        public bool UseTimer => true;
        public int TimeLimitSeconds => 100;
    }
}