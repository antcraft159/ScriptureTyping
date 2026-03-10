// 파일명: Models/ClozeQuestion.cs
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.Cloze.Models
{
    /// <summary>
    /// 목적:
    /// 빈칸 채우기 게임의 한 문제 전체를 표현하는 모델.
    /// 
    /// 포함 정보:
    /// - 원본 문장
    /// - 빈칸 처리된 문장
    /// - 정답 목록
    /// - 보기 목록
    /// - 난이도/모드 이름
    /// </summary>
    public sealed class ClozeQuestion
    {
        /// <summary>
        /// 원본 구절/문장
        /// </summary>
        public string OriginalText { get; init; } = string.Empty;

        /// <summary>
        /// 빈칸 처리된 문장
        /// 예: "태초에 ____이 천지를 창조하시니라"
        /// </summary>
        public string MaskedText { get; init; } = string.Empty;

        /// <summary>
        /// 문제에 포함된 정답 목록
        /// </summary>
        public IReadOnlyList<ClozeAnswer> Answers { get; init; } = new List<ClozeAnswer>();

        /// <summary>
        /// 빈칸별 보기 세트 목록
        /// </summary>
        public IReadOnlyList<ClozeOptionSet> OptionSets { get; init; } = new List<ClozeOptionSet>();

        /// <summary>
        /// 모드 이름
        /// 예: Easy, Normal, Hard
        /// </summary>
        public string ModeName { get; init; } = string.Empty;

        /// <summary>
        /// 총 빈칸 수
        /// </summary>
        public int BlankCount => Answers?.Count ?? 0;
    }
}