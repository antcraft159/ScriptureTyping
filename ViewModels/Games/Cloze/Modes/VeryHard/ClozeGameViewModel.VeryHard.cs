namespace ScriptureTyping.ViewModels.Games
{
    public sealed partial class ClozeGameViewModel
    {
        private bool IsVeryHardDifficulty()
        {
            return CurrentDifficulty == DIFFICULTY_VERY_HARD;
        }

        private int GetVeryHardBlankCount()
        {
            return 2;
        }

        private int GetVeryHardChoiceCount()
        {
            return 5;
        }

        private int GetVeryHardTryCount()
        {
            return 1;
        }

        private int GetVeryHardCorrectScore()
        {
            return 16;
        }

        private int GetVeryHardWrongPenalty()
        {
            return 4;
        }

        private bool IsVeryHardTimeAttack()
        {
            return true;
        }

        private int GetVeryHardTimeAttackSeconds()
        {
            return 10;
        }
    }
}