// 파일명: Models/ClozeOptionSet.cs
using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.Cloze.Models
{
    /// <summary>
    /// 목적:
    /// 빈칸 하나에 대응되는 보기 목록을 저장하는 모델.
    /// 
    /// 예:
    /// 첫 번째 빈칸:
    /// - 하나님
    /// - 사람
    /// - 하늘
    /// - 말씀
    /// - ...
    /// </summary>
    public sealed class ClozeOptionSet
    {
        /// <summary>
        /// 몇 번째 빈칸의 보기인지
        /// </summary>
        public int BlankIndex { get; init; }

        /// <summary>
        /// 보기 목록
        /// </summary>
        public IReadOnlyList<string> Options { get; init; } = new List<string>();

        /// <summary>
        /// 정답 텍스트
        /// UI 검증이나 디버깅 용도
        /// </summary>
        public string CorrectOption { get; init; } = string.Empty;
    }
}