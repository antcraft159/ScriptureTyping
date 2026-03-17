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
    /// 쉬움 난이도용 WordOrderQuestion을 생성한다.
    ///
    /// 특징:
    /// - 첫 조각 고정
    /// - 방해 조각 없음
    /// - 힌트 2회
    /// - 타이머 없음
    /// </summary>
    public sealed class EasyQuestionGenerator : IWordOrderQuestionGenerator
    {
        public string Difficulty => WordOrderDifficulty.Easy;

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
            IReadOnlyList<WordOrderPieceItem> pieces =
                pieceBuilder.BuildPieces(verse, sourceVerses, correctSequence);

            return new WordOrderQuestion
            {
                ReferenceText = verse.Ref,
                Difficulty = WordOrderDifficulty.Easy,
                CorrectSequence = correctSequence.ToList(),
                Pieces = pieces.ToList(),
                HintCount = 2,
                UseTimer = false,
                TimeLimitSeconds = 0,
                IsFirstPieceFixed = true
            };
        }
    }
}