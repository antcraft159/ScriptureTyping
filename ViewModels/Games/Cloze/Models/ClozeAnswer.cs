// 파일명: Models/ClozeAnswer.cs
namespace ScriptureTyping.ViewModels.Games.Cloze.Models
{
    /// <summary>
    /// 목적:
    /// 빈칸 하나에 대한 정답 정보를 저장하는 모델.
    /// 
    /// 예:
    /// - BlankIndex: 0
    /// - Text: "하나님"
    /// - TokenIndex: 2
    /// </summary>
    public sealed class ClozeAnswer
    {
        /// <summary>
        /// 빈칸 순서(0부터 시작)
        /// </summary>
        public int BlankIndex { get; init; }

        /// <summary>
        /// 정답 텍스트
        /// </summary>
        public string Text { get; init; } = string.Empty;

        /// <summary>
        /// 원본 토큰 배열에서의 위치
        /// </summary>
        public int TokenIndex { get; init; }

        /// <summary>
        /// 표시용 정답인지 여부
        /// 필요 시 힌트/특수 모드 확장용
        /// </summary>
        public bool IsVisibleHint { get; init; }
    }
}