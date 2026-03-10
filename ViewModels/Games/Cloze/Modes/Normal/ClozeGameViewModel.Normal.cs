using System;
using System.Linq;
using System.Windows.Input;

namespace ScriptureTyping.ViewModels.Games
{
    public sealed partial class ClozeGameViewModel
    {
        private string? _selectedFirstChoice;
        private string? _selectedSecondChoice;

        public string? SelectedFirstChoice
        {
            get => _selectedFirstChoice;
            private set
            {
                if (_selectedFirstChoice == value) return;
                _selectedFirstChoice = value;
                OnPropertyChanged();
            }
        }

        public string? SelectedSecondChoice
        {
            get => _selectedSecondChoice;
            private set
            {
                if (_selectedSecondChoice == value) return;
                _selectedSecondChoice = value;
                OnPropertyChanged();
            }
        }

        public bool AreDualChoicesVisible =>
            _current != null &&
            _current.IsDualBlank &&
            FirstChoices.Count > 0 &&
            SecondChoices.Count > 0;

        public bool AreFirstChoicesEnabled =>
            CanAnswerChoices() &&
            _current != null &&
            _current.IsDualBlank &&
            FirstChoices.Count > 0;

        public bool AreSecondChoicesEnabled =>
            CanAnswerChoices() &&
            _current != null &&
            _current.IsDualBlank &&
            SecondChoices.Count > 0;

        public bool IsNormalMode => CurrentDifficulty == DIFFICULTY_NORMAL;

        public ICommand SelectFirstChoiceCommand { get; }
        public ICommand SelectSecondChoiceCommand { get; }

        private void SelectFirstChoice(string? choice)
        {
            if (choice == null || _current == null || !_current.IsDualBlank || !AreFirstChoicesEnabled)
            {
                return;
            }

            SelectedFirstChoice = choice.Trim();

            if (string.IsNullOrWhiteSpace(SelectedSecondChoice))
            {
                FeedbackText = $"첫 번째 빈칸 선택 완료. 두 번째 빈칸을 고르세요. (기회 {_tryLeft}회)";
                RaiseUiComputed();
                return;
            }

            EvaluateDualSelection();
        }

        private void SelectSecondChoice(string? choice)
        {
            if (choice == null || _current == null || !_current.IsDualBlank || !AreSecondChoicesEnabled)
            {
                return;
            }

            SelectedSecondChoice = choice.Trim();

            if (string.IsNullOrWhiteSpace(SelectedFirstChoice))
            {
                FeedbackText = $"두 번째 빈칸 선택 완료. 첫 번째 빈칸을 고르세요. (기회 {_tryLeft}회)";
                RaiseUiComputed();
                return;
            }

            EvaluateDualSelection();
        }

        private void EvaluateDualSelection()
        {
            if (_current == null ||
                !_current.IsDualBlank ||
                string.IsNullOrWhiteSpace(SelectedFirstChoice) ||
                string.IsNullOrWhiteSpace(SelectedSecondChoice))
            {
                return;
            }

            bool firstCorrect = string.Equals(SelectedFirstChoice.Trim(), _current.Answers[0].Trim(), StringComparison.Ordinal);
            bool secondCorrect = string.Equals(SelectedSecondChoice.Trim(), _current.Answers[1].Trim(), StringComparison.Ordinal);

            if (firstCorrect && secondCorrect)
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
                FeedbackText = $"기회 소진. 정답은 \"{string.Join(", ", _current.Answers)}\" 입니다.";
                StopTimer();
                RaiseUiComputed();
                ScheduleAutoNext();
                return;
            }

            SelectedFirstChoice = null;
            SelectedSecondChoice = null;
            FeedbackText = $"틀렸습니다. 다시 선택하세요. (기회 {_tryLeft}회)";
            RaiseUiComputed();
        }

        private string BuildInitialGuideText()
        {
            if (_current == null)
            {
                return string.Empty;
            }

            if (_current.IsDualBlank)
            {
                return $"위/아래 보기에서 각 빈칸의 정답을 고르세요. (기회 {_tryLeft}회)";
            }

            return $"보기에서 정답을 고르세요. (기회 {_tryLeft}회)";
        }

        private int GetBlankCount()
        {
            return CurrentDifficulty switch
            {
                DIFFICULTY_NORMAL => 2,
                DIFFICULTY_HARD => 2,
                DIFFICULTY_VERY_HARD => 2,
                DIFFICULTY_SAMUEL_RANK1 => 2,
                _ => 1
            };
        }

        private List<string> SelectAnswersByDifficulty(IReadOnlyList<string> candidates, int count)
        {
            List<string> pool = candidates.Distinct(StringComparer.Ordinal).ToList();

            if (CurrentDifficulty == DIFFICULTY_HARD ||
                CurrentDifficulty == DIFFICULTY_VERY_HARD ||
                CurrentDifficulty == DIFFICULTY_SAMUEL_RANK1 ||
                CurrentDifficulty == DIFFICULTY_NORMAL)
            {
                pool = pool
                    .OrderByDescending(x => x.Length)
                    .ThenBy(_ => _rng.Next())
                    .ToList();
            }
            else
            {
                Shuffle(pool);
            }

            return pool.Take(count).ToList();
        }

        private int GetChoiceCount()
        {
            return CurrentDifficulty switch
            {
                DIFFICULTY_EASY => 6,
                DIFFICULTY_NORMAL => 6,
                DIFFICULTY_HARD => 6,
                DIFFICULTY_VERY_HARD => 5,
                DIFFICULTY_SAMUEL_RANK1 => 4,
                _ => 6
            };
        }

        private int GetTryCount()
        {
            return CurrentDifficulty switch
            {
                DIFFICULTY_EASY => 2,
                DIFFICULTY_NORMAL => 2,
                DIFFICULTY_HARD => 1,
                DIFFICULTY_VERY_HARD => 1,
                DIFFICULTY_SAMUEL_RANK1 => 1,
                _ => 2
            };
        }

        private int GetCorrectScore()
        {
            return CurrentDifficulty switch
            {
                DIFFICULTY_EASY => 10,
                DIFFICULTY_NORMAL => 12,
                DIFFICULTY_HARD => 14,
                DIFFICULTY_VERY_HARD => 16,
                DIFFICULTY_SAMUEL_RANK1 => 20,
                _ => 10
            };
        }

        private int GetWrongPenalty()
        {
            return CurrentDifficulty switch
            {
                DIFFICULTY_EASY => 2,
                DIFFICULTY_NORMAL => 2,
                DIFFICULTY_HARD => 3,
                DIFFICULTY_VERY_HARD => 4,
                DIFFICULTY_SAMUEL_RANK1 => 5,
                _ => 2
            };
        }

        private bool IsTimeAttackDifficulty(string difficulty)
        {
            return difficulty == DIFFICULTY_VERY_HARD || difficulty == DIFFICULTY_SAMUEL_RANK1;
        }

        private int GetTimeAttackSeconds()
        {
            return CurrentDifficulty switch
            {
                DIFFICULTY_VERY_HARD => 10,
                DIFFICULTY_SAMUEL_RANK1 => 8,
                _ => 15
            };
        }
    }
}