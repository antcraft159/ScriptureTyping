using ScriptureTyping.Commands;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ScriptureTyping.ViewModels.Games
{
    public sealed partial class ClozeGameViewModel
    {
        private static readonly TimeSpan SAMUEL_PREVIEW_DELAY = TimeSpan.FromMilliseconds(1200);

        private RelayCommand? _submitSamuelRank1AnswerCommand;
        private string _samuelRank1InputText = string.Empty;

        public string SamuelRank1InputText
        {
            get => _samuelRank1InputText;
            set
            {
                string nextValue = value ?? string.Empty;

                if (_samuelRank1InputText == nextValue)
                {
                    return;
                }

                _samuelRank1InputText = nextValue;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSubmitSamuelRank1Answer));

                try
                {
                    CommandManager.InvalidateRequerySuggested();
                }
                catch
                {
                }
            }
        }

        public bool IsSamuelRank1InputVisible =>
            IsSamuelRank1Mode &&
            _current != null &&
            !_isPreviewing &&
            !_isRoundCompleted;

        public bool CanSubmitSamuelRank1Answer =>
            IsSamuelRank1InputVisible &&
            !_isCorrect &&
            _tryLeft > 0 &&
            !string.IsNullOrWhiteSpace(SamuelRank1InputText);

        public ICommand SubmitSamuelRank1AnswerCommand =>
            _submitSamuelRank1AnswerCommand ??= new RelayCommand(
                _ => SubmitSamuelRank1Answer(),
                _ => CanSubmitSamuelRank1Answer);

        private bool IsSamuelRank1Mode =>
            string.Equals(CurrentDifficulty, DIFFICULTY_SAMUEL_RANK1, StringComparison.Ordinal);

        private int GetSamuelRank1BlankCount()
        {
            return 0;
        }

        private int GetSamuelRank1ChoiceCount()
        {
            return 0;
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
            return 150;
        }

        private void SubmitSamuelRank1Answer()
        {
            if (_current == null || !CanSubmitSamuelRank1Answer)
            {
                return;
            }

            string submitted = NormalizeSamuelRank1Text(SamuelRank1InputText);
            string answer = NormalizeSamuelRank1Text(_current.OriginalText);

            if (string.Equals(submitted, answer, StringComparison.Ordinal))
            {
                HandleCorrectAnswer();
                return;
            }

            _isCorrect = false;
            _tryLeft -= 1;
            _score = Math.Max(0, _score - GetWrongPenalty());
            _combo = 0;

            if (_tryLeft <= 0)
            {
                FeedbackText = $"오답. 정답은 \"{_current.OriginalText}\" 입니다.";
                StopTimer();
                RaiseUiComputed();
                ScheduleAutoNext();
                return;
            }

            FeedbackText = $"오답입니다. 남은 기회 {_tryLeft}회";
            RaiseUiComputed();
        }

        private static string NormalizeSamuelRank1Text(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            string normalized = text.Trim();
            normalized = Regex.Replace(normalized, @"\s+", "");
            normalized = Regex.Replace(normalized, @"[^\p{L}\p{N}]", "");

            return normalized;
        }
    }
}