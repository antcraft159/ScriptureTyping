using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.Easy
{
    /// <summary>
    /// 목적:
    /// 쉬움 단계용 문제를 생성한다.
    /// </summary>
    public sealed class EasyQuestionGenerator : IWordOrderQuestionGenerator
    {
        public string Difficulty => WordOrderDifficulty.Easy;

        public WordOrderQuestion Generate(
            Verse verse,
            IReadOnlyList<Verse> sourceVerses,
            IWordOrderPieceBuilder pieceBuilder,
            int hintCount,
            bool useTimer,
            int timeLimitSeconds,
            bool isFirstPieceFixed)
        {
            if (verse is null)
            {
                throw new ArgumentNullException(nameof(verse));
            }

            if (sourceVerses is null)
            {
                throw new ArgumentNullException(nameof(sourceVerses));
            }

            if (pieceBuilder is null)
            {
                throw new ArgumentNullException(nameof(pieceBuilder));
            }

            IReadOnlyList<string> correctSequence = pieceBuilder.BuildCorrectSequence(verse);

            if (correctSequence.Count == 0)
            {
                correctSequence = new List<string>
                {
                    (verse.Text ?? string.Empty).Trim()
                };
            }

            IReadOnlyList<WordOrderPieceItem> pieces = pieceBuilder.BuildPieces(
                verse,
                sourceVerses,
                correctSequence);

            return new WordOrderQuestion
            {
                Difficulty = Difficulty,
                ReferenceText = verse.Ref ?? string.Empty,
                VerseText = verse.Text ?? string.Empty,
                CorrectSequence = correctSequence.ToList(),
                Pieces = pieces.ToList(),
                HintCount = hintCount,
                UseTimer = useTimer,
                TimeLimitSeconds = timeLimitSeconds,
                IsFirstPieceFixed = isFirstPieceFixed
            };
        }
    }
}