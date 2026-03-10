namespace ScriptureTyping.ViewModels.Games.Cloze.Models
{
    /// <summary>
    /// 목적:
    /// 단어 분석 결과를 담는다.
    /// </summary>
    public sealed class ClozeWordAnalysisResult
    {
        public bool IsValid { get; init; }

        public string Word { get; init; } = string.Empty;

        public string Stem { get; init; } = string.Empty;

        public string Ending { get; init; } = string.Empty;

        public int GroupIndex { get; init; } = -1;

        public ClozeWordType WordType { get; init; } = ClozeWordType.Unknown;

        public string Particle { get; init; } = string.Empty;

        public bool IsHonorific { get; init; }

        public bool IsPast { get; init; }

        public bool IsFuture { get; init; }

        public bool IsCopula { get; init; }
    }
}