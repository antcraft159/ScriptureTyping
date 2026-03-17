using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.Normal
{
    /// <summary>
    /// 목적:
    /// 보통 단계용 문제를 생성한다.
    ///
    /// 규칙:
    /// - 기본은 공백 기준 어절 분리
    /// - 조각 수가 너무 적으면 PieceBuilder 정책을 따른다
    /// - 방해 조각은 PieceBuilder 정책에 따라 추가할 수 있다
    /// </summary>
    public sealed class NormalQuestionGenerator : IWordOrderQuestionGenerator
    {
        public string Difficulty => WordOrderDifficulty.Normal;

        public WordOrderQuestion Generate(
            Verse verse,
            IReadOnlyList<Verse> sourceVerses,
            IWordOrderPieceBuilder pieceBuilder)
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
                HintCount = 2,
                UseTimer = false,
                TimeLimitSeconds = 0,
                IsFirstPieceFixed = false
            };
        }
    }
}