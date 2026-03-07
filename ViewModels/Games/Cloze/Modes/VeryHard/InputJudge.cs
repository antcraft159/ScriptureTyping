// 파일명: VeryHard/InputJudge.cs
namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.VeryHard
{
    /// <summary>
    /// 목적:
    /// 매우 어려움 모드에서 입력칸 하나의 판정 결과를 저장한다.
    /// 
    /// 사용 예:
    /// - 첫 번째 입력칸 정답 여부
    /// - 두 번째 입력칸 정답 여부
    /// </summary>
    public sealed class InputJudge
    {
        /// <summary>
        /// 몇 번째 입력칸인지 (0부터 시작)
        /// </summary>
        public int BlankIndex { get; init; }

        /// <summary>
        /// 사용자가 입력한 값
        /// </summary>
        public string Submitted { get; init; } = string.Empty;

        /// <summary>
        /// 실제 정답
        /// </summary>
        public string Expected { get; init; } = string.Empty;

        /// <summary>
        /// 정답 여부
        /// </summary>
        public bool IsCorrect { get; init; }
    }
}