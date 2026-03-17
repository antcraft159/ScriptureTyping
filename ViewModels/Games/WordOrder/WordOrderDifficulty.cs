using System.Collections.Generic;

namespace ScriptureTyping.ViewModels.Games.WordOrder
{
    /// <summary>
    /// 목적:
    /// 순서 맞추기 게임에서 사용하는 난이도 문자열을 한 곳에서 관리한다.
    ///
    /// 주요 역할:
    /// - 난이도 이름 상수 제공
    /// - 난이도 목록 제공
    ///
    /// 주의사항:
    /// - UI 콤보박스, ViewModel 기본값, ModeFactory 분기에서 동일한 값을 사용해야 한다.
    /// - 기존 WordOrderDifficultyRules에 있던 "문자열 상수 역할"만 이 클래스로 분리한다.
    /// </summary>
    public static class WordOrderDifficulty
    {
        public const string Easy = "쉬움";
        public const string Normal = "보통";
        public const string Hard = "어려움";
        public const string VeryHard = "매우 어려움";

        /// <summary>
        /// UI 표시 및 난이도 순회에 사용하는 전체 난이도 목록
        /// </summary>
        public static IReadOnlyList<string> All { get; } = new[]
        {
            Easy,
            Normal,
            Hard,
            VeryHard
        };
    }
}