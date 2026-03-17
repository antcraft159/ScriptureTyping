using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Models
{
    /// <summary>
    /// 목적:
    /// 순서 맞추기 1문제의 전체 데이터를 담는다.
    ///
    /// 역할:
    /// - 장절
    /// - 원문
    /// - 정답 조각 순서
    /// - 보기 조각 목록
    /// - 힌트/타이머/고정 조각 여부
    /// </summary>
    public sealed class WordOrderQuestion
    {
        /// <summary>
        /// 난이도
        /// </summary>
        public string Difficulty { get; set; } = string.Empty;

        /// <summary>
        /// 화면에 보여줄 장절
        /// 예: 요 3:16
        /// </summary>
        public string ReferenceText { get; set; } = string.Empty;

        /// <summary>
        /// 원본 말씀 전체 텍스트
        /// </summary>
        public string VerseText { get; set; } = string.Empty;

        /// <summary>
        /// 정답 조각 순서
        /// </summary>
        public List<string> CorrectSequence { get; set; } = new List<string>();

        /// <summary>
        /// 사용자에게 보여줄 전체 조각 목록
        /// </summary>
        public List<WordOrderPieceItem> Pieces { get; set; } = new List<WordOrderPieceItem>();

        /// <summary>
        /// 사용 가능한 힌트 수
        /// </summary>
        public int HintCount { get; set; }

        /// <summary>
        /// 타이머 사용 여부
        /// </summary>
        public bool UseTimer { get; set; }

        /// <summary>
        /// 제한 시간(초)
        /// </summary>
        public int TimeLimitSeconds { get; set; }

        /// <summary>
        /// 첫 조각 고정 여부
        /// </summary>
        public bool IsFirstPieceFixed { get; set; }
    }
}