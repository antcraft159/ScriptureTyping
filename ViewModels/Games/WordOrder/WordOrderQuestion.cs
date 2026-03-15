using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder
{
    /// <summary>
    /// 목적:
    /// 순서 맞추기 게임의 문제 1개를 표현한다.
    /// </summary>
    public sealed class WordOrderQuestion
    {
        public string Difficulty { get; init; } = string.Empty;

        public string ReferenceText { get; init; } = string.Empty;

        public string OriginalText { get; init; } = string.Empty;

        public List<string> CorrectSequence { get; init; } = new();

        public List<WordOrderPieceItem> Pieces { get; init; } = new();

        public int HintCount { get; init; }

        public bool UseTimer { get; init; }

        public int TimeLimitSeconds { get; init; }

        public bool IsFirstPieceFixed { get; init; }

        public bool IsLastPieceFixed { get; init; }

        public bool ShowSlotNumbers { get; init; }

        public bool ShowCorrectPositionFeedback { get; init; }
    }
}