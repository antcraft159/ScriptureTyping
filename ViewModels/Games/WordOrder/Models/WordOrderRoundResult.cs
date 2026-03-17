using System;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Models
{
    /// <summary>
    /// 목적:
    /// 한 문제(라운드)의 결과를 기록한다.
    ///
    /// 주요 역할:
    /// - 몇 번째 문제인지 기록
    /// - 장절 기록
    /// - 정답 여부 기록
    /// - 제출 횟수/힌트 사용 횟수/남은 시간 기록
    /// - 최종 피드백 문구 기록
    ///
    /// 주의사항:
    /// - 통계 화면이나 결과 요약 화면을 만들 때 사용할 수 있다.
    /// </summary>
    public sealed class WordOrderRoundResult
    {
        /// <summary>
        /// 목적:
        /// 0-based 현재 문제 인덱스를 보관한다.
        /// </summary>
        public int QuestionIndex { get; init; }

        /// <summary>
        /// 목적:
        /// 사용자에게 보여준 장절 정보를 보관한다.
        /// </summary>
        public string ReferenceText { get; init; } = string.Empty;

        /// <summary>
        /// 목적:
        /// 현재 문제의 난이도 정보를 보관한다.
        /// </summary>
        public string Difficulty { get; init; } = string.Empty;

        /// <summary>
        /// 목적:
        /// 최종 정답 여부를 보관한다.
        /// </summary>
        public bool IsCorrect { get; init; }

        /// <summary>
        /// 목적:
        /// 제한 시간 초과 여부를 보관한다.
        /// </summary>
        public bool IsTimeout { get; init; }

        /// <summary>
        /// 목적:
        /// 실제 제출한 횟수를 보관한다.
        /// </summary>
        public int SubmitCount { get; init; }

        /// <summary>
        /// 목적:
        /// 사용한 힌트 횟수를 보관한다.
        /// </summary>
        public int UsedHintCount { get; init; }

        /// <summary>
        /// 목적:
        /// 문제 종료 시점의 남은 시간(초)을 보관한다.
        /// </summary>
        public int RemainingSeconds { get; init; }

        /// <summary>
        /// 목적:
        /// 문제 종료 시 사용자에게 표시한 최종 피드백 문구를 보관한다.
        /// </summary>
        public string FeedbackText { get; init; } = string.Empty;

        /// <summary>
        /// 목적:
        /// 사용자 제출 답안을 보관한다.
        /// </summary>
        public WordOrderAnswer Answer { get; init; } = new WordOrderAnswer();
    }
}