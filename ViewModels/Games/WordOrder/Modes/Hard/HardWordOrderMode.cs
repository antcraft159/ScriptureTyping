using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.Hard
{
    /// <summary>
    /// 목적:
    /// Hard 난이도의 전체 규칙을 묶는 모드 객체다.
    ///
    /// 담당:
    /// - 문제 생성기 제공
    /// - 채점 정책 제공
    /// - 힌트 정책 제공
    /// - 조각 생성기 제공
    /// - 제출 횟수 / 힌트 / 타이머 / 안내 문구 같은 모드 규칙 제공
    /// </summary>
    public sealed class HardWordOrderMode : IWordOrderMode
    {
        private const int MAX_SUBMIT_COUNT = 2;
        private const int HINT_COUNT = 1;
        private const int TIME_LIMIT_SECONDS = 45;

        public HardWordOrderMode()
        {
            QuestionGenerator = new HardQuestionGenerator();
            ScoringPolicy = new HardScoringPolicy();
            HintPolicy = new HardHintPolicy();
            PieceBuilder = new HardPieceBuilder();
        }

        public string Difficulty => WordOrderDifficulty.Hard;

        /// <summary>
        /// 문제 생성기
        /// </summary>
        public IWordOrderQuestionGenerator QuestionGenerator { get; }

        /// <summary>
        /// 채점 정책
        /// </summary>
        public IWordOrderScoringPolicy ScoringPolicy { get; }

        /// <summary>
        /// 힌트 정책
        /// </summary>
        public IWordOrderHintPolicy HintPolicy { get; }

        /// <summary>
        /// 조각 생성기
        /// </summary>
        public IWordOrderPieceBuilder PieceBuilder { get; }

        public int MaxSubmitCount => MAX_SUBMIT_COUNT;

        public int HintCount => HINT_COUNT;

        public bool UseTimer => true;

        public int TimeLimitSeconds => TIME_LIMIT_SECONDS;

        public bool IsFirstPieceFixed => false;

        public string GetInitialGuideText()
        {
            return "어절 순서를 정확히 배열하세요. 방해 조각에 주의하세요.";
        }

        public string GetCorrectFeedbackText()
        {
            return "정답입니다.";
        }

        public string GetWrongFeedbackText(
            WordOrderQuestion question,
            IReadOnlyList<WordOrderPieceItem> answerPieces,
            bool containsDistractor)
        {
            if (question is null)
            {
                throw new ArgumentNullException(nameof(question));
            }

            if (answerPieces is null)
            {
                throw new ArgumentNullException(nameof(answerPieces));
            }

            if (containsDistractor)
            {
                return "오답입니다. 방해 조각이 포함되어 있습니다.";
            }

            if (answerPieces.Count != question.CorrectSequence.Count)
            {
                return "오답입니다. 조각 수가 맞지 않습니다.";
            }

            return "오답입니다. 어절 순서를 다시 확인하세요.";
        }

        public WordOrderQuestion CreateQuestion(Verse verse, IReadOnlyList<Verse> sourceVerses)
        {
            if (verse is null)
            {
                throw new ArgumentNullException(nameof(verse));
            }

            if (sourceVerses is null)
            {
                throw new ArgumentNullException(nameof(sourceVerses));
            }

            return QuestionGenerator.Generate(
                verse,
                sourceVerses,
                PieceBuilder);
        }

        public bool IsAnswerCorrect(WordOrderQuestion question, IReadOnlyList<WordOrderPieceItem> answerPieces)
        {
            if (question is null)
            {
                throw new ArgumentNullException(nameof(question));
            }

            if (answerPieces is null)
            {
                throw new ArgumentNullException(nameof(answerPieces));
            }

            return ScoringPolicy.IsCorrect(question, answerPieces);
        }

        public bool TryApplyHint(
            WordOrderQuestion question,
            IList<WordOrderPieceItem> availablePieces,
            IList<WordOrderPieceItem> answerPieces,
            out string message)
        {
            if (question is null)
            {
                throw new ArgumentNullException(nameof(question));
            }

            if (availablePieces is null)
            {
                throw new ArgumentNullException(nameof(availablePieces));
            }

            if (answerPieces is null)
            {
                throw new ArgumentNullException(nameof(answerPieces));
            }

            return HintPolicy.TryApplyHint(question, availablePieces, answerPieces, out message);
        }
    }
}