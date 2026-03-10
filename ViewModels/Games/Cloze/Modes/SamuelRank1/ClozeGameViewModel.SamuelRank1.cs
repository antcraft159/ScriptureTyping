using System;
using System.Threading.Tasks;

namespace ScriptureTyping.ViewModels.Games
{
    public sealed partial class ClozeGameViewModel
    {
        private static readonly TimeSpan SAMUEL_PREVIEW_DELAY = TimeSpan.FromMilliseconds(1200);

        private bool IsSamuelRank1Mode =>
            string.Equals(CurrentDifficulty, DIFFICULTY_SAMUEL_RANK1, StringComparison.Ordinal);

        private int GetSamuelRank1BlankCount()
        {
            return 2;
        }

        private int GetSamuelRank1ChoiceCount()
        {
            return 4;
        }

        private int GetSamuelRank1TryCount()
        {
            return 1;
        }

        private int GetSamuelRank1CorrectScore()
        {
            return 20;
        }

        private int GetSamuelRank1WrongPenalty()
        {
            return 5;
        }

        private bool IsSamuelRank1TimeAttack()
        {
            return true;
        }

        private int GetSamuelRank1TimeAttackSeconds()
        {
            return 8;
        }

        private async Task<bool> RunSamuelRank1PreviewAsync(int localVersion)
        {
            if (_current == null)
            {
                return false;
            }

            _isPreviewing = true;
            QuestionText = _current.OriginalText;
            FeedbackText = "잠깐 보여줍니다. 집중해서 기억하세요.";
            _timeLeftSec = 0;
            RaiseUiComputed();

            await Task.Delay(SAMUEL_PREVIEW_DELAY);

            if (localVersion != _questionVersion || _current == null || _isRoundCompleted)
            {
                return false;
            }

            _isPreviewing = false;
            QuestionText = _current.ClozeText;
            FeedbackText = BuildInitialGuideText();

            return true;
        }
    }
}