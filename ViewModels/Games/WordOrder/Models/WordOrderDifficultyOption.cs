namespace ScriptureTyping.ViewModels.Games.WordOrder.Models
{
    /// <summary>
    /// 목적:
    /// 순서 맞추기 난이도 선택 UI에서 사용할 옵션 1개를 표현한다.
    ///
    /// 주요 역할:
    /// - 화면 표시명 보관
    /// - 실제 내부 난이도 값 보관
    /// - 설명 문구 보관
    ///
    /// 주의사항:
    /// - ComboBox 바인딩 시 DisplayName을 보여주고 Value를 실제 값으로 사용할 수 있다.
    /// </summary>
    public sealed class WordOrderDifficultyOption
    {
        /// <summary>
        /// 목적:
        /// UI에 표시할 난이도 이름을 보관한다.
        /// 예: "쉬움"
        /// </summary>
        public string DisplayName { get; init; } = string.Empty;

        /// <summary>
        /// 목적:
        /// 내부 로직에서 사용할 난이도 값을 보관한다.
        /// 보통 DisplayName과 같게 둘 수 있다.
        /// </summary>
        public string Value { get; init; } = string.Empty;

        /// <summary>
        /// 목적:
        /// 난이도 설명 문구를 보관한다.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// 목적:
        /// 디버깅이나 문자열 표시 시 DisplayName을 반환한다.
        /// </summary>
        public override string ToString()
        {
            return DisplayName;
        }
    }
}