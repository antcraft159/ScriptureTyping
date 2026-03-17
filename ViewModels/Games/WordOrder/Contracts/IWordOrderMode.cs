using ScriptureTyping.Data;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Contracts
{
    /// <summary>
    /// 목적:
    /// 순서 맞추기 난이도(모드) 1개의 전체 규칙 묶음을 정의한다.
    ///
    /// 역할:
    /// - 문제 생성기 제공
    /// - 채점 정책 제공
    /// - 힌트 정책 제공
    /// - 조각 생성기 제공
    /// - 난이도별 기본 설정값 제공
    /// - 난이도별 안내/피드백 문구 제공
    ///
    /// 입력:
    /// - verse: 문제 생성 대상 말씀
    /// - sourceVerses: 방해 조각 생성 등에 사용할 전체 말씀 목록
    /// - question: 현재 문제
    /// - answerPieces: 사용자가 배치한 답 조각 목록
    /// - availablePieces: 아직 선택하지 않은 조각 목록
    ///
    /// 출력:
    /// - 문제 생성 결과
    /// - 정답 여부
    /// - 힌트 적용 여부 및 메시지
    ///
    /// 주의사항:
    /// - ViewModel은 세부 규칙을 직접 구현하지 않고 이 인터페이스만 호출해야 한다.
    /// - 난이도별 차이는 구현체(Easy/Normal/Hard/VeryHard)에서 처리한다.
    /// </summary>
    public interface IWordOrderMode
    {
        /// <summary>
        /// 현재 모드의 난이도 이름
        /// 예: 쉬움, 보통, 어려움, 매우 어려움
        /// </summary>
        string Difficulty { get; }

        /// <summary>
        /// 문제당 최대 제출 가능 횟수
        /// </summary>
        int MaxSubmitCount { get; }

        /// <summary>
        /// 문제당 사용 가능한 힌트 수
        /// </summary>
        int HintCount { get; }

        /// <summary>
        /// 타이머 사용 여부
        /// </summary>
        bool UseTimer { get; }

        /// <summary>
        /// 제한 시간(초)
        /// UseTimer가 false면 0이어도 된다.
        /// </summary>
        int TimeLimitSeconds { get; }

        /// <summary>
        /// 첫 조각을 고정 배치할지 여부
        /// </summary>
        bool IsFirstPieceFixed { get; }

        /// <summary>
        /// 현재 모드에서 사용할 문제 생성기
        /// </summary>
        IWordOrderQuestionGenerator QuestionGenerator { get; }

        /// <summary>
        /// 현재 모드에서 사용할 채점 정책
        /// </summary>
        IWordOrderScoringPolicy ScoringPolicy { get; }

        /// <summary>
        /// 현재 모드에서 사용할 힌트 정책
        /// </summary>
        IWordOrderHintPolicy HintPolicy { get; }

        /// <summary>
        /// 현재 모드에서 사용할 조각 생성기
        /// </summary>
        IWordOrderPieceBuilder PieceBuilder { get; }

        /// <summary>
        /// 게임 시작 또는 문제 시작 시 보여줄 기본 안내 문구를 반환한다.
        /// </summary>
        string GetInitialGuideText();

        /// <summary>
        /// 정답 제출 시 보여줄 피드백 문구를 반환한다.
        /// </summary>
        string GetCorrectFeedbackText();

        /// <summary>
        /// 오답 제출 시 보여줄 피드백 문구를 반환한다.
        /// </summary>
        /// <param name="question">현재 문제</param>
        /// <param name="answerPieces">사용자가 제출한 답 조각 목록</param>
        /// <param name="containsDistractor">방해 조각 포함 여부</param>
        string GetWrongFeedbackText(
            WordOrderQuestion question,
            IReadOnlyList<WordOrderPieceItem> answerPieces,
            bool containsDistractor);

        /// <summary>
        /// 현재 모드 규칙으로 문제를 생성한다.
        /// </summary>
        /// <param name="verse">문제 원본 말씀</param>
        /// <param name="sourceVerses">전체 말씀 소스</param>
        WordOrderQuestion CreateQuestion(Verse verse, IReadOnlyList<Verse> sourceVerses);

        /// <summary>
        /// 현재 모드 채점 규칙으로 정답 여부를 판정한다.
        /// </summary>
        /// <param name="question">현재 문제</param>
        /// <param name="answerPieces">사용자 답안 조각</param>
        bool IsAnswerCorrect(
            WordOrderQuestion question,
            IReadOnlyList<WordOrderPieceItem> answerPieces);

        /// <summary>
        /// 현재 모드 힌트 규칙으로 힌트를 적용한다.
        /// </summary>
        /// <param name="question">현재 문제</param>
        /// <param name="availablePieces">현재 남아 있는 조각 목록</param>
        /// <param name="answerPieces">현재 배치된 답안 조각 목록</param>
        /// <param name="message">실행 결과 메시지</param>
        /// <returns>힌트 적용 성공 여부</returns>
        bool TryApplyHint(
            WordOrderQuestion question,
            IList<WordOrderPieceItem> availablePieces,
            IList<WordOrderPieceItem> answerPieces,
            out string message);
    }
}