using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.SamuelRank1
{
    /// <summary>
    /// 목적:
    /// SamuelRank1 난이도 전체 정책을 하나로 묶는다.
    ///
    /// 포함 역할:
    /// - 문제 생성
    /// - 채점
    /// - 힌트 적용
    /// - 단계별 메타데이터 제공
    ///
    /// 규칙:
    /// - 매우 어려움 기반
    /// - 첫 조각 고정 없음
    /// - 타이머 사용
    /// - 힌트 없음
    /// - 제출 기회 1회
    /// - 후치사/조사만 바뀐 방해 조각 추가
    /// </summary>
    public sealed class SamuelRank1WordOrderMode : IWordOrderMode
    {
        private const int DEFAULT_MAX_SUBMIT_COUNT = 1;
        private const int DEFAULT_HINT_COUNT = 0;
        private const bool DEFAULT_USE_TIMER = true;
        private const int DEFAULT_TIME_LIMIT_SECONDS = 60;
        private const bool DEFAULT_IS_FIRST_PIECE_FIXED = false;

        public SamuelRank1WordOrderMode()
        {
            PieceBuilder = new SamuelRank1PieceBuilder();
            QuestionGenerator = new SamuelRank1QuestionGenerator();
            ScoringPolicy = new SamuelRank1ScoringPolicy();
            HintPolicy = new SamuelRank1HintPolicy();
        }

        /// <summary>
        /// 목적:
        /// 현재 모드가 담당하는 난이도를 반환한다.
        /// </summary>
        public string Difficulty => WordOrderDifficulty.SamuelRank1;

        /// <summary>
        /// 목적:
        /// 최대 제출 횟수를 반환한다.
        /// </summary>
        public int MaxSubmitCount => DEFAULT_MAX_SUBMIT_COUNT;

        /// <summary>
        /// 목적:
        /// 시작 힌트 개수를 반환한다.
        /// </summary>
        public int HintCount => DEFAULT_HINT_COUNT;

        /// <summary>
        /// 목적:
        /// 타이머 사용 여부를 반환한다.
        /// </summary>
        public bool UseTimer => DEFAULT_USE_TIMER;

        /// <summary>
        /// 목적:
        /// 제한 시간을 반환한다.
        /// </summary>
        public int TimeLimitSeconds => DEFAULT_TIME_LIMIT_SECONDS;

        /// <summary>
        /// 목적:
        /// 첫 조각 고정 여부를 반환한다.
        /// </summary>
        public bool IsFirstPieceFixed => DEFAULT_IS_FIRST_PIECE_FIXED;

        /// <summary>
        /// 목적:
        /// 현재 모드에서 사용할 문제 생성기
        /// </summary>
        public IWordOrderQuestionGenerator QuestionGenerator { get; }

        /// <summary>
        /// 목적:
        /// 현재 모드에서 사용할 채점 정책
        /// </summary>
        public IWordOrderScoringPolicy ScoringPolicy { get; }

        /// <summary>
        /// 목적:
        /// 현재 모드에서 사용할 힌트 정책
        /// </summary>
        public IWordOrderHintPolicy HintPolicy { get; }

        /// <summary>
        /// 목적:
        /// 현재 모드에서 사용할 조각 생성기
        /// </summary>
        public IWordOrderPieceBuilder PieceBuilder { get; }

        /// <summary>
        /// 목적:
        /// SamuelRank1 시작 안내 문구를 반환한다.
        /// </summary>
        public string GetInitialGuideText()
        {
            return "사무엘 1등 단계입니다. 매우 어려움 규칙에 더해 후치사만 바뀐 방해 조각이 추가됩니다.";
        }

        /// <summary>
        /// 목적:
        /// SamuelRank1 정답 피드백 문구를 반환한다.
        /// </summary>
        public string GetCorrectFeedbackText()
        {
            return "정답입니다! 후치사 방해 조각까지 정확히 구분했습니다.";
        }

        /// <summary>
        /// 목적:
        /// SamuelRank1 오답 피드백 문구를 반환한다.
        /// </summary>
        public string GetWrongFeedbackText(
            WordOrderQuestion question,
            IReadOnlyList<WordOrderPieceItem> answerPieces,
            bool containsDistractor)
        {
            if (containsDistractor)
            {
                return "오답입니다. 후치사만 바뀐 방해 조각이 포함되어 있습니다.";
            }

            return "오답입니다. 비슷한 후치사 조각까지 다시 구분해 보세요.";
        }

        /// <summary>
        /// 목적:
        /// SamuelRank1 문제를 생성한다.
        /// </summary>
        public WordOrderQuestion CreateQuestion(Verse verse, IReadOnlyList<Verse> sourceVerses)
        {
            return QuestionGenerator.Generate(
                verse,
                sourceVerses,
                PieceBuilder,
                HintCount,
                UseTimer,
                TimeLimitSeconds,
                IsFirstPieceFixed);
        }

        /// <summary>
        /// 목적:
        /// 현재 답안이 정답인지 판정한다.
        /// </summary>
        public bool IsAnswerCorrect(
            WordOrderQuestion question,
            IReadOnlyList<WordOrderPieceItem> answerPieces)
        {
            return ScoringPolicy.IsCorrect(question, answerPieces);
        }

        /// <summary>
        /// 목적:
        /// 힌트 적용을 시도한다.
        ///
        /// 규칙:
        /// - SamuelRank1은 힌트를 허용하지 않는다.
        /// </summary>
        public bool TryApplyHint(
            WordOrderQuestion question,
            IList<WordOrderPieceItem> availablePieces,
            IList<WordOrderPieceItem> answerPieces,
            out string message)
        {
            return HintPolicy.TryApplyHint(
                question,
                availablePieces,
                answerPieces,
                out message);
        }
    }
}