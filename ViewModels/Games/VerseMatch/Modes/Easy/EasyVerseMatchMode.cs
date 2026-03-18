using ScriptureTyping.ViewModels.Games.VerseMatch.Contracts;

namespace ScriptureTyping.ViewModels.Games.VerseMatch.Modes.Easy
{
    /// <summary>
    /// 목적:
    /// 쉬움 난이도 정책을 제공한다.
    /// </summary>
    public sealed class EasyVerseMatchMode : IVerseMatchMode
    {
        public string Difficulty => VerseMatchDifficulty.Easy;
        public int PairCount => 3;
        public int PreviewLength => 22;
        public int FakeCardCount => 0;
        public bool UseTimer => false;
        public int TimeLimitSeconds => 0;
    }
}