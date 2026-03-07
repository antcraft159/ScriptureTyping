using System;
using System.Threading.Tasks;

namespace ScriptureTyping.ViewModels.Games
{
    public sealed partial class ClozeGameViewModel
    {
        private static readonly TimeSpan SAMUEL_PREVIEW_DELAY = TimeSpan.FromMilliseconds(1200);

        private bool IsSamuelRank1Mode =>
            string.Equals(CurrentDifficulty, DIFFICULTY_SAMUEL_RANK1, StringComparison.Ordinal);

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