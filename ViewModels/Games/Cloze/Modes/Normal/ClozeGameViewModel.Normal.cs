using ScriptureTyping.Commands;
using ScriptureTyping.ViewModels.Games.Cloze.Models;
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
                if (_selectedFirstChoice == value)
                {
                    return;
                }

                _selectedFirstChoice = value;
                OnPropertyChanged();
            }
        }

        public string? SelectedSecondChoice
        {
            get => _selectedSecondChoice;
            private set
            {
                if (_selectedSecondChoice == value)
                {
                    return;
                }

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

        /// <summary>
        /// 목적:
        /// ChoiceGroups 기반 공통 다중 빈칸 보기 선택 가능 여부를 판단한다.
        /// </summary>
        public bool AreChoiceGroupsEnabled =>
            CanAnswerChoices() &&
            _current != null &&
            ChoiceGroups.Count > 0;

        public bool IsNormalMode => CurrentDifficulty == DIFFICULTY_NORMAL;

        public ICommand SelectFirstChoiceCommand { get; }
        public ICommand SelectSecondChoiceCommand { get; }

        /// <summary>
        /// 목적:
        /// 공통 다중 빈칸 보기 그룹에서 선택지를 고를 때 사용하는 커맨드
        /// </summary>
        public ICommand SelectChoiceGroupCommand =>
            _selectChoiceGroupCommand ??= new RelayCommand(
                p => ExecuteSelectChoiceGroup(p),
                _ => AreChoiceGroupsEnabled);

        private RelayCommand? _selectChoiceGroupCommand;

        private bool IsNormalDifficulty()
        {
            return CurrentDifficulty == DIFFICULTY_NORMAL;
        }

        private int GetNormalBlankCount()
        {
            return 5;
        }

        private int GetNormalChoiceCount()
        {
            return 6;
        }

        private int GetNormalTryCount()
        {
            return 2;
        }

        private int GetNormalCorrectScore()
        {
            return 12;
        }

        private int GetNormalWrongPenalty()
        {
            return 2;
        }

        private bool IsNormalTimeAttack()
        {
            return false;
        }

        private int GetNormalTimeAttackSeconds()
        {
            return 15;
        }

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

        /// <summary>
        /// 목적:
        /// ChoiceGroups에서 선택이 들어오면 해당 그룹에 선택값을 반영한다.
        /// 모든 그룹이 선택되면 정답 판정을 수행한다.
        /// </summary>
        private void ExecuteSelectChoiceGroup(object? parameter)
        {
            if (!AreChoiceGroupsEnabled || parameter is not object[] values || values.Length != 2)
            {
                return;
            }

            if (values[0] is not ClozeChoiceGroupItem group)
            {
                return;
            }

            if (values[1] is not string choice || string.IsNullOrWhiteSpace(choice))
            {
                return;
            }

            group.SelectedChoice = choice.Trim();

            bool allSelected = ChoiceGroups.All(x => !string.IsNullOrWhiteSpace(x.SelectedChoice));

            if (!allSelected)
            {
                FeedbackText = $"모든 빈칸을 선택하세요. (기회 {_tryLeft}회)";
                RaiseUiComputed();
                return;
            }

            EvaluateChoiceGroupsSelection();
        }

        /// <summary>
        /// 목적:
        /// ChoiceGroups에 담긴 다중 빈칸 선택 결과를 정답과 비교한다.
        /// </summary>
        private void EvaluateChoiceGroupsSelection()
        {
            if (_current == null)
            {
                return;
            }

            if (ChoiceGroups.Count != _current.Answers.Count)
            {
                return;
            }

            bool allCorrect = true;

            for (int i = 0; i < ChoiceGroups.Count; i++)
            {
                string selected = ChoiceGroups[i].SelectedChoice?.Trim() ?? string.Empty;
                string expected = _current.Answers[i].Trim();

                if (!string.Equals(selected, expected, StringComparison.Ordinal))
                {
                    allCorrect = false;
                    break;
                }
            }

            if (allCorrect)
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

            foreach (ClozeChoiceGroupItem item in ChoiceGroups)
            {
                item.SelectedChoice = null;
            }

            FeedbackText = $"틀렸습니다. 다시 선택하세요. (기회 {_tryLeft}회)";
            RaiseUiComputed();
        }

        private string BuildInitialGuideText()
        {
            if (_current == null)
            {
                return string.Empty;
            }

            if (ChoiceGroups.Count > 0)
            {
                return $"각 빈칸의 정답을 모두 고르세요. (기회 {_tryLeft}회)";
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
                DIFFICULTY_EASY => GetEasyBlankCount(),
                DIFFICULTY_NORMAL => GetNormalBlankCount(),
                DIFFICULTY_HARD => GetHardBlankCount(),
                DIFFICULTY_VERY_HARD => GetVeryHardBlankCount(),
                DIFFICULTY_SAMUEL_RANK1 => GetSamuelRank1BlankCount(),
                _ => GetEasyBlankCount()
            };
        }

        private List<string> SelectAnswersByDifficulty(IReadOnlyList<string> candidates, int count)
        {
            if (CurrentDifficulty == DIFFICULTY_EASY)
            {
                return SelectEasyAnswers(candidates, count);
            }

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
                DIFFICULTY_EASY => GetEasyChoiceCount(),
                DIFFICULTY_NORMAL => GetNormalChoiceCount(),
                DIFFICULTY_HARD => GetHardChoiceCount(),
                DIFFICULTY_VERY_HARD => GetVeryHardChoiceCount(),
                DIFFICULTY_SAMUEL_RANK1 => GetSamuelRank1ChoiceCount(),
                _ => GetEasyChoiceCount()
            };
        }

        private int GetTryCount()
        {
            return CurrentDifficulty switch
            {
                DIFFICULTY_EASY => GetEasyTryCount(),
                DIFFICULTY_NORMAL => GetNormalTryCount(),
                DIFFICULTY_HARD => GetHardTryCount(),
                DIFFICULTY_VERY_HARD => GetVeryHardTryCount(),
                DIFFICULTY_SAMUEL_RANK1 => GetSamuelRank1TryCount(),
                _ => GetEasyTryCount()
            };
        }

        private int GetCorrectScore()
        {
            return CurrentDifficulty switch
            {
                DIFFICULTY_EASY => GetEasyCorrectScore(),
                DIFFICULTY_NORMAL => GetNormalCorrectScore(),
                DIFFICULTY_HARD => GetHardCorrectScore(),
                DIFFICULTY_VERY_HARD => GetVeryHardCorrectScore(),
                DIFFICULTY_SAMUEL_RANK1 => GetSamuelRank1CorrectScore(),
                _ => GetEasyCorrectScore()
            };
        }

        private int GetWrongPenalty()
        {
            return CurrentDifficulty switch
            {
                DIFFICULTY_EASY => GetEasyWrongPenalty(),
                DIFFICULTY_NORMAL => GetNormalWrongPenalty(),
                DIFFICULTY_HARD => GetHardWrongPenalty(),
                DIFFICULTY_VERY_HARD => GetVeryHardWrongPenalty(),
                DIFFICULTY_SAMUEL_RANK1 => GetSamuelRank1WrongPenalty(),
                _ => GetEasyWrongPenalty()
            };
        }

        private bool IsTimeAttackDifficulty(string difficulty)
        {
            return difficulty switch
            {
                DIFFICULTY_EASY => IsEasyTimeAttack(),
                DIFFICULTY_NORMAL => IsNormalTimeAttack(),
                DIFFICULTY_HARD => IsHardTimeAttack(),
                DIFFICULTY_VERY_HARD => IsVeryHardTimeAttack(),
                DIFFICULTY_SAMUEL_RANK1 => IsSamuelRank1TimeAttack(),
                _ => false
            };
        }

        private int GetTimeAttackSeconds()
        {
            return CurrentDifficulty switch
            {
                DIFFICULTY_EASY => GetEasyTimeAttackSeconds(),
                DIFFICULTY_NORMAL => GetNormalTimeAttackSeconds(),
                DIFFICULTY_HARD => GetHardTimeAttackSeconds(),
                DIFFICULTY_VERY_HARD => GetVeryHardTimeAttackSeconds(),
                DIFFICULTY_SAMUEL_RANK1 => GetSamuelRank1TimeAttackSeconds(),
                _ => 15
            };
        }
    }
}