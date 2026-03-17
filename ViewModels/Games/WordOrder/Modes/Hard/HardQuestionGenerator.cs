using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.Hard
{
    /// <summary>
    /// 목적:
    /// Hard 난이도용 WordOrderQuestion을 생성한다.
    ///
    /// 규칙:
    /// - 정답 순서는 공백 기준 어절 단위
    /// - 첫 조각 고정 없음
    /// - 타이머 사용 여부는 모드 설정을 따른다
    /// - 방해 조각 포함 가능
    /// </summary>
    public sealed class HardQuestionGenerator : IWordOrderQuestionGenerator
    {
        /// <summary>
        /// 목적:
        /// 현재 문제 생성기가 담당하는 난이도를 나타낸다.
        /// </summary>
        public string Difficulty => WordOrderDifficulty.Hard;

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