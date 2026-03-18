using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.Normal
{
    /// <summary>
    /// 목적:
    /// 순서 맞추기 "보통" 단계의 전체 규칙을 묶어서 제공한다.
    ///
    /// 역할:
    /// - 문제 생성기 제공
    /// - 채점 정책 제공
    /// - 힌트 정책 제공
    /// - 조각 생성기 제공
    /// - 난이도별 안내/피드백/제한값 제공
    /// </summary>
    public sealed class NormalWordOrderMode : IWordOrderMode
    {
        public NormalWordOrderMode()
            : this(
                  new NormalPieceBuilder(),
                  new NormalQuestionGenerator(),
                  new NormalScoringPolicy(),
                  new NormalHintPolicy())
        {
        }

        public NormalWordOrderMode(
            IWordOrderPieceBuilder pieceBuilder,
            IWordOrderQuestionGenerator questionGenerator,
            IWordOrderScoringPolicy scoringPolicy,
            IWordOrderHintPolicy hintPolicy)
        {
            PieceBuilder = pieceBuilder ?? throw new ArgumentNullException(nameof(pieceBuilder));
            QuestionGenerator = questionGenerator ?? throw new ArgumentNullException(nameof(questionGenerator));
            ScoringPolicy = scoringPolicy ?? throw new ArgumentNullException(nameof(scoringPolicy));
            HintPolicy = hintPolicy ?? throw new ArgumentNullException(nameof(hintPolicy));
        }

        public string Difficulty => WordOrderDifficulty.Normal;

        public IWordOrderQuestionGenerator QuestionGenerator { get; }

        public IWordOrderScoringPolicy ScoringPolicy { get; }

        public IWordOrderHintPolicy HintPolicy { get; }

        public IWordOrderPieceBuilder PieceBuilder { get; }

        public int MaxSubmitCount => 2;

        public int HintCount => 1;

        public bool UseTimer => false;

        public int TimeLimitSeconds => 0;

        public bool IsFirstPieceFixed => false;

        public string GetInitialGuideText()
        {
            return "보기를 눌러 순서대로 문장을 완성하세요.";
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
                return "방해 조각이 포함되었습니다.";
            }

            if (answerPieces.Count != question.CorrectSequence.Count)
            {
                return "조각 수가 맞지 않습니다.";
            }

            return "순서가 맞지 않습니다.";
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
        PieceBuilder,
        HintCount,
        UseTimer,
        TimeLimitSeconds,
        IsFirstPieceFixed);
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