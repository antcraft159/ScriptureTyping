using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.VeryHard
{
    /// <summary>
    /// 목적:
    /// 매우 어려움 단계의 규칙을 한 곳에서 묶는다.
    /// </summary>
    public sealed class VeryHardWordOrderMode : IWordOrderMode
    {
        private const int DEFAULT_MAX_SUBMIT_COUNT = 2;
        private const int DEFAULT_HINT_COUNT = 1;
        private const int DEFAULT_TIME_LIMIT_SECONDS = 20;

        public VeryHardWordOrderMode()
            : this(
                  new VeryHardQuestionGenerator(),
                  new VeryHardScoringPolicy(),
                  new VeryHardHintPolicy(),
                  new VeryHardPieceBuilder())
        {
        }

        public VeryHardWordOrderMode(
            IWordOrderQuestionGenerator questionGenerator,
            IWordOrderScoringPolicy scoringPolicy,
            IWordOrderHintPolicy hintPolicy,
            IWordOrderPieceBuilder pieceBuilder)
        {
            QuestionGenerator = questionGenerator ?? throw new ArgumentNullException(nameof(questionGenerator));
            ScoringPolicy = scoringPolicy ?? throw new ArgumentNullException(nameof(scoringPolicy));
            HintPolicy = hintPolicy ?? throw new ArgumentNullException(nameof(hintPolicy));
            PieceBuilder = pieceBuilder ?? throw new ArgumentNullException(nameof(pieceBuilder));
        }

        public string Difficulty => WordOrderDifficulty.VeryHard;

        /// <summary>
        /// IWordOrderMode 요구 멤버
        /// </summary>
        public IWordOrderQuestionGenerator QuestionGenerator { get; }

        /// <summary>
        /// IWordOrderMode 요구 멤버
        /// </summary>
        public IWordOrderScoringPolicy ScoringPolicy { get; }

        /// <summary>
        /// IWordOrderMode 요구 멤버
        /// </summary>
        public IWordOrderHintPolicy HintPolicy { get; }

        /// <summary>
        /// IWordOrderMode 요구 멤버
        /// </summary>
        public IWordOrderPieceBuilder PieceBuilder { get; }

        public int MaxSubmitCount => DEFAULT_MAX_SUBMIT_COUNT;

        public int HintCount => DEFAULT_HINT_COUNT;

        public bool UseTimer => true;

        public int TimeLimitSeconds => DEFAULT_TIME_LIMIT_SECONDS;

        public bool IsFirstPieceFixed => false;

        public string GetInitialGuideText()
        {
            return "방해 조각에 주의하면서 순서를 맞춰보세요.";
        }

        public string GetCorrectFeedbackText()
        {
            return "정답입니다! 방해 조각 없이 정확히 맞췄습니다.";
        }

        public string GetWrongFeedbackText(
            WordOrderQuestion question,
            IReadOnlyList<WordOrderPieceItem> answerPieces,
            bool containsDistractor)
        {
            if (containsDistractor)
            {
                return "오답입니다. 방해 조각이 포함되어 있습니다.";
            }

            return "오답입니다. 조각 순서를 다시 확인하세요.";
        }

        public WordOrderQuestion CreateQuestion(Verse verse, IReadOnlyList<Verse> sourceVerses)
        {
            return QuestionGenerator.Generate(verse, sourceVerses, PieceBuilder);
        }

        public bool IsAnswerCorrect(WordOrderQuestion question, IReadOnlyList<WordOrderPieceItem> answerPieces)
        {
            return ScoringPolicy.IsCorrect(question, answerPieces);
        }

        public bool TryApplyHint(
            WordOrderQuestion question,
            IList<WordOrderPieceItem> availablePieces,
            IList<WordOrderPieceItem> answerPieces,
            out string message)
        {
            return HintPolicy.TryApplyHint(question, availablePieces, answerPieces, out message);
        }
    }
}