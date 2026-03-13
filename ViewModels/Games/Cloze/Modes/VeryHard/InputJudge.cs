namespace ScriptureTyping.ViewModels.Games.Cloze.Modes.VeryHard
{
    /// <summary>
    /// 목적:
    /// 매우 어려움 모드에서 빈칸 1개의 입력 판정 결과를 저장한다.
    ///
    /// 설명:
    /// - 사용자가 입력한 값
    /// - 실제 정답
    /// - 정규화된 비교값
    /// - 정답 여부
    /// 를 함께 보관한다.
    /// </summary>
    public sealed class InputJudge
    {
        /// <summary>
        /// 목적:
        /// 몇 번째 빈칸인지 나타낸다.
        /// 
        /// 설명:
        /// 내부 로직에서는 0부터 시작한다.
        /// 예:
        /// - 0 = 첫 번째 빈칸
        /// - 1 = 두 번째 빈칸
        /// </summary>
        public int BlankIndex { get; init; }

        /// <summary>
        /// 목적:
        /// 사용자가 실제로 입력한 원본 문자열을 보관한다.
        /// </summary>
        public string Submitted { get; init; } = string.Empty;

        /// <summary>
        /// 목적:
        /// 정답 원본 문자열을 보관한다.
        /// </summary>
        public string Expected { get; init; } = string.Empty;

        /// <summary>
        /// 목적:
        /// 비교를 위해 정규화한 사용자 입력값을 보관한다.
        ///
        /// 예:
        /// - 앞뒤 공백 제거
        /// - 내부 공백 제거
        /// - 줄바꿈 제거
        /// </summary>
        public string NormalizedSubmitted { get; init; } = string.Empty;

        /// <summary>
        /// 목적:
        /// 비교를 위해 정규화한 정답값을 보관한다.
        /// </summary>
        public string NormalizedExpected { get; init; } = string.Empty;

        /// <summary>
        /// 목적:
        /// 현재 입력칸이 정답인지 여부를 나타낸다.
        /// </summary>
        public bool IsCorrect { get; init; }

        /// <summary>
        /// 목적:
        /// UI 표시용 1-based 번호를 반환한다.
        /// </summary>
        public int DisplayIndex => BlankIndex + 1;
    }
}