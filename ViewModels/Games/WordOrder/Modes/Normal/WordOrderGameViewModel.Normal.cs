using ScriptureTyping.ViewModels.Games.WordOrder.Contracts;

namespace ScriptureTyping.ViewModels.Games.WordOrder.Modes.Normal
{
    public sealed partial class WordOrderGameViewModel
    {
        private static bool IsNormalDifficulty(string difficulty)
        {
            return string.Equals(difficulty, WordOrderDifficulty.Normal, System.StringComparison.Ordinal);
        }

        private static IWordOrderMode CreateNormalMode()
        {
            return new NormalWordOrderMode();
        }
    }
}