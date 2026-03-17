using ScriptureTyping.ViewModels.Games.WordOrder.Modes.VeryHard;

namespace ScriptureTyping.ViewModels.Games.WordOrder
{
    /// <summary>
    /// 목적:
    /// WordOrderGameViewModel의 매우 어려움 관련 보조 확장 메서드를 둔다.
    /// </summary>
    public sealed partial class WordOrderGameViewModel
    {
        private bool IsVeryHardDifficulty()
        {
            return string.Equals(
                SelectedDifficulty,
                WordOrderDifficulty.VeryHard,
                System.StringComparison.Ordinal);
        }

        private VeryHardWordOrderMode CreateVeryHardMode()
        {
            return new VeryHardWordOrderMode();
        }
    }
}