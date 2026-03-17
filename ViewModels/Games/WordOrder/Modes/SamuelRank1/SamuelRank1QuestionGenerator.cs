using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.SamuelRank1
{
    /// <summary>
    /// 목적:
    /// SamuelRank1 난이도용 WordOrderQuestion을 생성한다.
    ///
    /// 규칙:
    /// - 정답 조각과 보기 조각 생성은 PieceBuilder에 위임한다.
    /// - 모드에서 전달한 힌트/타이머/첫 조각 고정 설정을 그대로 문제 객체에 반영한다.
    /// </summary>
    public sealed class SamuelRank1QuestionGenerator : IWordOrderQuestionGenerator
    {
        /// <summary>
        /// 목적:
        /// 현재 문제 생성기가 담당하는 난이도를 나타낸다.
        /// </summary>
        public string Difficulty => WordOrderDifficulty.SamuelRank1;

        /// <summary>
        /// 목적:
        /// SamuelRank1 규칙에 맞는 문제를 생성한다.
        /// </summary>
        /// <param name="verse">문제 원본 말씀</param>
        /// <param name="sourceVerses">전체 말씀 소스</param>
        /// <param name="pieceBuilder">조각 생성기</param>
        /// <param name="hintCount">허용 힌트 수</param>
        /// <param name="useTimer">타이머 사용 여부</param>
        /// <param name="timeLimitSeconds">제한 시간(초)</param>
        /// <param name="isFirstPieceFixed">첫 조각 고정 여부</param>
        /// <returns>생성된 순서 맞추기 문제</returns>
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
                throw new InvalidOperationException("말씀 본문을 조각으로 분리할 수 없습니다.");
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