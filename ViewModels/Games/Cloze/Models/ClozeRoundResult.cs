// 파일명: Models/ClozeRoundResult.cs
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.Cloze.Models
{
    /// <summary>
    /// 목적:
    /// 한 문제(한 라운드)에 대한 채점 결과를 저장하는 모델.
    /// 
    /// 포함 정보:
    /// - 전체 정답 여부
    /// - 부분 정답 개수
    /// - 제출 답안
    /// - 실제 정답
    /// - 점수
    /// - 결과 메시지
    /// </summary>
    public sealed class ClozeRoundResult
    {
        /// <summary>
        /// 전체 정답 여부
        /// </summary>
        public bool IsCorrect { get; init; }

        /// <summary>
        /// 맞춘 개수
        /// </summary>
        public int CorrectCount { get; init; }

        /// <summary>
        /// 전체 빈칸 개수
        /// </summary>
        public int TotalCount { get; init; }

        /// <summary>
        /// 점수
        /// </summary>
        public int Score { get; init; }

        /// <summary>
        /// 사용자가 제출한 답안 목록
        /// </summary>
        public IReadOnlyList<string> SubmittedAnswers { get; init; } = new List<string>();

        /// <summary>
        /// 실제 정답 목록
        /// </summary>
        public IReadOnlyList<string> CorrectAnswers { get; init; } = new List<string>();

        /// <summary>
        /// 빈칸별 정답 여부
        /// </summary>
        public IReadOnlyList<bool> PerBlankResults { get; init; } = new List<bool>();

        /// <summary>
        /// 결과 메시지
        /// </summary>
        public string Message { get; init; } = string.Empty;
    }
}