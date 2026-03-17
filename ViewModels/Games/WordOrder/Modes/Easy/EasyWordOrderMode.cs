using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.Easy
{
    /// <summary>
    /// 목적:
    /// 쉬움 난이도 전체 정책을 하나로 묶는다.
    ///
    /// 포함 역할:
    /// - 문제 생성
    /// - 채점
    /// - 힌트 적용
    /// - 쉬움 단계 메타데이터 제공
    /// </summary>
    public sealed class EasyWordOrderMode : IWordOrderMode
    {
        private readonly EasyQuestionGenerator _questionGenerator;
        private readonly EasyScoringPolicy _scoringPolicy;
        private readonly EasyHintPolicy _hintPolicy;
        private readonly EasyPieceBuilder _pieceBuilder;

        public EasyWordOrderMode()
        {
            _pieceBuilder = new EasyPieceBuilder();

            _questionGenerator = new EasyQuestionGenerator();
            _scoringPolicy = new EasyScoringPolicy();
            _hintPolicy = new EasyHintPolicy();
        }

        public string Difficulty => WordOrderDifficulty.Easy;

        public int MaxSubmitCount => 2;

        public int HintCount => 2;

        public bool UseTimer => false;

        public int TimeLimitSeconds => 0;

        public bool IsFirstPieceFixed => true;

        public IWordOrderQuestionGenerator QuestionGenerator => _questionGenerator;

        public IWordOrderScoringPolicy ScoringPolicy => _scoringPolicy;

        public IWordOrderHintPolicy HintPolicy => _hintPolicy;

        public IWordOrderPieceBuilder PieceBuilder => _pieceBuilder;

        public string GetInitialGuideText()
        {
            return "첫 조각이 고정됩니다. 나머지 큰 조각들의 순서를 맞춰보세요.";
        }

        public string GetCorrectFeedbackText()
        {
            return "정답입니다! 쉬움 단계 통과입니다.";
        }

        public string GetWrongFeedbackText(
            WordOrderQuestion question,
            IReadOnlyList<WordOrderPieceItem> answerPieces,
            bool containsDistractor)
        {
            return _scoringPolicy.BuildWrongFeedbackText(question, answerPieces, containsDistractor);
        }

        public WordOrderQuestion CreateQuestion(Verse verse, IReadOnlyList<Verse> sourceVerses)
        {
            return _questionGenerator.Generate(verse, sourceVerses, _pieceBuilder);
        }

        public bool IsAnswerCorrect(WordOrderQuestion question, IReadOnlyList<WordOrderPieceItem> answerPieces)
        {
            return _scoringPolicy.IsCorrect(question, answerPieces);
        }

        public bool TryApplyHint(
            WordOrderQuestion question,
            IList<WordOrderPieceItem> availablePieces,
            IList<WordOrderPieceItem> answerPieces,
            out string message)
        {
            return _hintPolicy.TryApplyHint(question, availablePieces, answerPieces, out message);
        }
    }
}