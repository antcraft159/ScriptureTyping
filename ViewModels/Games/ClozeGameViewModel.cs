using ScriptureTyping.Commands;
using ScriptureTyping.Data;
using ScriptureTyping.Services;
using ScriptureTyping.ViewModels.Games.Cloze.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace ScriptureTyping.ViewModels.Games
{
    public sealed partial class ClozeGameViewModel : INotifyPropertyChanged
    {
        private const string DEFAULT_TITLE = "빈칸 채우기";
        private const string ALL_DAY_TEXT = "전일차";
        private const int ALL_DAY_INDEX = 0;

        private const string DIFFICULTY_EASY = "쉬움";
        private const string DIFFICULTY_NORMAL = "보통";
        private const string DIFFICULTY_HARD = "어려움";
        private const string DIFFICULTY_VERY_HARD = "매우 어려움";
        private const string DIFFICULTY_SAMUEL_RANK1 = "사무엘 1등";

        private static readonly TimeSpan AUTO_NEXT_DELAY = TimeSpan.FromMilliseconds(2000);

        private static readonly HashSet<string> JosaVariantBlockedWords = new(StringComparer.Ordinal)
        {
            "그러므로",
            "그러나",
            "또한",
            "만일",
            "이미",
            "오직",
            "곧",
            "참으로",
            "정녕",
            "진실로",
            "실로",
            "과연"
        };

        private readonly MainWindowViewModel? _host;
        private readonly SelectionContext _ctx;
        private readonly Random _rng = new Random();
        private readonly DispatcherTimer _timer;

        private readonly List<ClozeQuestion> _round = new List<ClozeQuestion>();
        private readonly List<string> _globalWordPool = new List<string>();

        private int _index;
        private int _score;
        private int _combo;
        private int _tryLeft;
        private bool _isCorrect;
        private bool _hintUsed;
        private bool _isRoundCompleted;
        private bool _isPreviewing;
        private int _timeLeftSec;

        private string _title = DEFAULT_TITLE;
        private string _questionText = string.Empty;
        private string _referenceText = string.Empty;
        private string _feedbackText = string.Empty;

        private string? _selectedCourse;
        private string? _selectedDay;
        private string? _selectedDifficulty;
        private string _selectionError = string.Empty;

        private ClozeQuestion? _current;
        private bool _isAutoNextScheduled;
        private int _questionVersion;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action? BackRequested;

        public ClozeGameViewModel()
            : this(host: null, selectionContext: App.SelectionContext)
        {
        }

        public ClozeGameViewModel(MainWindowViewModel host)
            : this(host, App.SelectionContext)
        {
        }

        public ClozeGameViewModel(SelectionContext selectionContext)
            : this(host: null, selectionContext)
        {
        }

        public ClozeGameViewModel(MainWindowViewModel? host, SelectionContext selectionContext)
        {
            _host = host;
            _ctx = selectionContext ?? throw new ArgumentNullException(nameof(selectionContext));

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (_, __) => OnTimerTick();

            BackCommand = new RelayCommand(_ => Back(), _ => true);
            SelectChoiceCommand = new RelayCommand(p => SelectChoice(p as string), _ => AreChoicesEnabled);
            SelectFirstChoiceCommand = new RelayCommand(p => SelectFirstChoice(p as string), _ => AreFirstChoicesEnabled);
            SelectSecondChoiceCommand = new RelayCommand(p => SelectSecondChoice(p as string), _ => AreSecondChoicesEnabled);
            HintCommand = new RelayCommand(_ => Hint(), _ => IsHintEnabled);
            RestartCommand = new RelayCommand(_ => Restart(), _ => true);
            ApplySelectionCommand = new RelayCommand(_ => ApplySelection(), _ => CanApplySelection());

            Title = DEFAULT_TITLE;

            InitSelectionUi();
            ApplySelectionFromContextOrDefault();
        }

        public ObservableCollection<string> Courses { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Days { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> DifficultyOptions { get; } = new ObservableCollection<string>();

        public ObservableCollection<string> Choices { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> FirstChoices { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> SecondChoices { get; } = new ObservableCollection<string>();
        public ObservableCollection<ClozeChoiceGroupItem> ChoiceGroups { get; } = new ObservableCollection<ClozeChoiceGroupItem>();

        public bool AreChoiceGroupsVisible =>
            !IsVeryHardInputVisible &&
            !IsSamuelRank1InputVisible &&
            _current != null &&
            ChoiceGroups.Count > 0;

        public string? SelectedCourse
        {
            get => _selectedCourse;
            set
            {
                if (_selectedCourse == value) return;
                _selectedCourse = value;
                OnPropertyChanged();
                SelectionError = string.Empty;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string? SelectedDay
        {
            get => _selectedDay;
            set
            {
                if (_selectedDay == value) return;
                _selectedDay = value;
                OnPropertyChanged();
                SelectionError = string.Empty;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string? SelectedDifficulty
        {
            get => _selectedDifficulty;
            set
            {
                if (_selectedDifficulty == value) return;
                _selectedDifficulty = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DifficultyText));
                OnPropertyChanged(nameof(IsNormalMode));
                OnPropertyChanged(nameof(IsVeryHardInputVisible));
                OnPropertyChanged(nameof(IsSamuelRank1InputVisible));
                OnPropertyChanged(nameof(CanSubmitSamuelRank1Answer));
                OnPropertyChanged(nameof(AreChoiceGroupsVisible));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string SelectionError
        {
            get => _selectionError;
            private set
            {
                if (_selectionError == value) return;
                _selectionError = value;
                OnPropertyChanged();
            }
        }

        public ICommand ApplySelectionCommand { get; }

        public string Title
        {
            get => _title;
            private set
            {
                if (_title == value) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        public string ProgressText => _round.Count <= 0 ? "0/0" : $"{Math.Min(_index + 1, _round.Count)}/{_round.Count}";
        public string ScoreText => $"점수 {_score}";
        public string ComboText => _combo > 0 ? $"콤보 x{_combo}" : "콤보 -";
        public string DifficultyText => $"모드 {CurrentDifficulty}";
        public bool IsTimeAttack { get; private set; }
        public string TimeLeftText => $"남은시간 {_timeLeftSec:00}s";

        public string QuestionText
        {
            get => _questionText;
            private set
            {
                if (_questionText == value) return;
                _questionText = value;
                OnPropertyChanged();
            }
        }

        public string ReferenceText
        {
            get => _referenceText;
            private set
            {
                if (_referenceText == value) return;
                _referenceText = value;
                OnPropertyChanged();
            }
        }

        public string FeedbackText
        {
            get => _feedbackText;
            private set
            {
                if (_feedbackText == value) return;
                _feedbackText = value;
                OnPropertyChanged();
            }
        }

        public bool AreSingleChoicesVisible =>
            !IsVeryHardInputVisible &&
            !IsSamuelRank1InputVisible &&
            _current != null &&
            !_current.IsDualBlank &&
            Choices.Count > 0;

        public bool AreChoicesEnabled =>
            !IsVeryHardInputVisible &&
            !IsSamuelRank1InputVisible &&
            CanAnswerChoices() &&
            _current != null &&
            !_current.IsDualBlank &&
            Choices.Count > 0;

        public bool IsHintEnabled =>
            !IsSamuelRank1Mode &&
            !_isRoundCompleted &&
            !_hintUsed &&
            !_isCorrect &&
            _tryLeft > 0 &&
            !_isPreviewing &&
            _current != null;

        public ICommand BackCommand { get; }
        public ICommand SelectChoiceCommand { get; }
        public ICommand HintCommand { get; }
        public ICommand RestartCommand { get; }

        private string CurrentDifficulty =>
            string.IsNullOrWhiteSpace(SelectedDifficulty)
                ? DIFFICULTY_EASY
                : SelectedDifficulty!;

        private void InitSelectionUi()
        {
            Courses.Clear();
            for (int i = 1; i <= VerseCatalog.MAX_COURSE; i++)
            {
                Courses.Add($"{i}과정");
            }

            Days.Clear();
            Days.Add(ALL_DAY_TEXT);
            for (int d = 1; d <= VerseCatalog.MAX_DAY; d++)
            {
                Days.Add($"{d}일차");
            }

            DifficultyOptions.Clear();
            DifficultyOptions.Add(DIFFICULTY_EASY);
            DifficultyOptions.Add(DIFFICULTY_NORMAL);
            DifficultyOptions.Add(DIFFICULTY_HARD);
            DifficultyOptions.Add(DIFFICULTY_VERY_HARD);
            DifficultyOptions.Add(DIFFICULTY_SAMUEL_RANK1);
        }

        private void ApplySelectionFromContextOrDefault()
        {
            SelectedDifficulty = DIFFICULTY_EASY;

            if (_ctx.HasSelection)
            {
                SelectedCourse = _ctx.SelectedCourseId;
                int dayIndex = _ctx.SelectedDayIndex ?? 0;
                SelectedDay = dayIndex == 0 ? ALL_DAY_TEXT : $"{dayIndex}일차";

                ApplySelection();
                return;
            }

            SelectedCourse = Courses.FirstOrDefault();
            SelectedDay = ALL_DAY_TEXT;

            SamuelRank1InputText = string.Empty;
            QuestionText = "상단에서 과정/일차/모드를 선택 후 [적용]을 누르세요.";
            ReferenceText = string.Empty;
            FeedbackText = string.Empty;
            ClearAllChoiceCollections();
            ClearVeryHardInputs();
            RaiseUiComputed();
        }

        private bool CanApplySelection()
        {
            return !string.IsNullOrWhiteSpace(SelectedCourse)
                && !string.IsNullOrWhiteSpace(SelectedDay)
                && !string.IsNullOrWhiteSpace(SelectedDifficulty);
        }

        private void ApplySelection()
        {
            if (string.IsNullOrWhiteSpace(SelectedCourse) ||
                string.IsNullOrWhiteSpace(SelectedDay) ||
                string.IsNullOrWhiteSpace(SelectedDifficulty))
            {
                SelectionError = "과정/일차/모드를 선택해 주세요.";
                return;
            }

            int courseNo = ParseCourseNo(SelectedCourse);
            int dayIndex = SelectedDay == ALL_DAY_TEXT ? ALL_DAY_INDEX : ParseDayNo(SelectedDay);

            IReadOnlyList<Verse> verses = BuildVerseList(courseNo, SelectedDay);

            if (verses.Count == 0)
            {
                SelectionError = "해당 선택에 구절이 없습니다.";
                SamuelRank1InputText = string.Empty;
                QuestionText = "구절이 없습니다.";
                ReferenceText = string.Empty;
                FeedbackText = string.Empty;
                ClearAllChoiceCollections();
                ClearVeryHardInputs();
                RaiseUiComputed();
                return;
            }

            List<VerseItem> items = verses
                .Select(v => new VerseItem { Ref = v.Ref, Text = v.Text })
                .ToList();

            _ctx.SetSelection(SelectedCourse, dayIndex, items);
            RestartRound(items);
        }

        private void RestartRound(IReadOnlyList<VerseItem> verses)
        {
            StopTimer();

            _round.Clear();
            _globalWordPool.Clear();
            _index = 0;
            _score = 0;
            _combo = 0;

            _tryLeft = GetTryCount();
            _isCorrect = false;
            _hintUsed = false;
            _isRoundCompleted = false;
            _isPreviewing = false;
            _isAutoNextScheduled = false;
            SelectedFirstChoice = null;
            SelectedSecondChoice = null;

            SamuelRank1InputText = string.Empty;
            ClearVeryHardInputs();

            IsTimeAttack = IsTimeAttackDifficulty(CurrentDifficulty);
            OnPropertyChanged(nameof(IsTimeAttack));
            OnPropertyChanged(nameof(DifficultyText));
            OnPropertyChanged(nameof(IsNormalMode));
            OnPropertyChanged(nameof(IsVeryHardInputVisible));
            OnPropertyChanged(nameof(IsSamuelRank1InputVisible));
            OnPropertyChanged(nameof(CanSubmitSamuelRank1Answer));
            OnPropertyChanged(nameof(AreChoiceGroupsVisible));

            BuildWordPool(verses);
            BuildRound(verses);

            if (_round.Count <= 0)
            {
                SamuelRank1InputText = string.Empty;
                QuestionText = "문제를 만들 수 있는 구절이 없습니다.";
                ReferenceText = string.Empty;
                FeedbackText = "다른 과정/일차를 선택해 주세요.";
                ClearAllChoiceCollections();
                ClearVeryHardInputs();
                RaiseUiComputed();
                return;
            }

            LoadQuestion(0);
        }

        private void Back()
        {
            StopTimer();

            if (_host != null)
            {
                _host.NavigateTo(new GamesHubViewModel(_host));
                return;
            }

            BackRequested?.Invoke();
        }

        private bool CanAnswerChoices()
        {
            return !IsSamuelRank1Mode &&
                   !_isRoundCompleted &&
                   !_isCorrect &&
                   _tryLeft > 0 &&
                   !_isPreviewing &&
                   _current != null;
        }

        private void SelectChoice(string? choice)
        {
            if (choice == null || _current == null || _current.IsDualBlank || !AreChoicesEnabled)
            {
                return;
            }

            if (string.Equals(choice.Trim(), _current.Answers[0].Trim(), StringComparison.Ordinal))
            {
                HandleCorrectAnswer();
                return;
            }

            HandleWrongSingleAnswer();
        }

        private void HandleCorrectAnswer()
        {
            _isCorrect = true;
            _score += GetCorrectScore();
            _combo += 1;

            FeedbackText = "정답!";
            StopTimer();
            RaiseUiComputed();
            ScheduleAutoNext();
        }

        private void HandleWrongSingleAnswer()
        {
            if (_current == null)
            {
                return;
            }

            _isCorrect = false;
            _tryLeft -= 1;
            _score = Math.Max(0, _score - GetWrongPenalty());
            _combo = 0;

            if (_tryLeft <= 0)
            {
                FeedbackText = $"기회 소진. 정답은 \"{_current.Answers[0]}\" 입니다.";
                StopTimer();
                RaiseUiComputed();
                ScheduleAutoNext();
                return;
            }

            FeedbackText = $"틀렸습니다. 다시 선택하세요. (기회 {_tryLeft}회)";
            RaiseUiComputed();
        }

        private async void ScheduleAutoNext()
        {
            if (_isAutoNextScheduled)
            {
                return;
            }

            _isAutoNextScheduled = true;

            try
            {
                await Task.Delay(AUTO_NEXT_DELAY);
                AutoNext();
            }
            finally
            {
                if (_isRoundCompleted)
                {
                    _isAutoNextScheduled = false;
                }
            }
        }

        private void AutoNext()
        {
            if (_isRoundCompleted)
            {
                _isAutoNextScheduled = false;
                return;
            }

            _index++;

            if (_index >= _round.Count)
            {
                _isRoundCompleted = true;
                StopTimer();

                SamuelRank1InputText = string.Empty;
                QuestionText = "라운드 종료!";
                ReferenceText = string.Empty;
                ClearAllChoiceCollections();
                ClearVeryHardInputs();
                FeedbackText = $"총 {_round.Count}문제 | 점수 {_score}";
                _isAutoNextScheduled = false;

                RaiseUiComputed();
                return;
            }

            LoadQuestion(_index);
        }

        private void Hint()
        {
            if (!IsHintEnabled || _current == null)
            {
                return;
            }

            _hintUsed = true;
            _score = Math.Max(0, _score - 1);

            if (IsVeryHardInputVisible)
            {
                ApplyVeryHardHint();
                FeedbackText = "힌트 사용(-1점).";
                RaiseUiComputed();
                return;
            }

            if (_current.IsDualBlank)
            {
                string hinted = _current.ClozeText;

                for (int i = 0; i < _current.Answers.Count; i++)
                {
                    string answer = _current.Answers[i];
                    string first = string.IsNullOrWhiteSpace(answer) ? string.Empty : answer.Substring(0, 1);
                    hinted = ReplacePlaceholderByOrder(hinted, i, $"{first}…");
                }

                QuestionText = hinted;
            }
            else
            {
                string first = string.IsNullOrWhiteSpace(_current.Answers[0])
                    ? string.Empty
                    : _current.Answers[0].Substring(0, 1);

                QuestionText = _current.ClozeText.Replace("____", $"{first}…");
            }

            FeedbackText = "힌트 사용(-1점).";
            RaiseUiComputed();
        }

        private void Restart()
        {
            ApplySelection();
        }

        private void BuildWordPool(IEnumerable<VerseItem> verses)
        {
            _globalWordPool.Clear();

            foreach (VerseItem verse in verses)
            {
                foreach (string word in ExtractCandidateWords(verse.Text))
                {
                    _globalWordPool.Add(word);
                }
            }
        }

        private void BuildRound(IReadOnlyList<VerseItem> verses)
        {
            _round.Clear();

            List<VerseItem> shuffled = verses.ToList();
            Shuffle(shuffled);

            foreach (VerseItem verse in shuffled)
            {
                if (!TryMakeQuestion(verse, out ClozeQuestion? question) || question == null)
                {
                    continue;
                }

                _round.Add(question);
            }

            Shuffle(_round);
        }

        private bool TryMakeQuestion(VerseItem verse, out ClozeQuestion? question)
        {
            question = null;

            if (IsSamuelRank1Mode)
            {
                return TryMakeSamuelRank1Question(verse, out question);
            }

            if (IsVeryHardDifficulty())
            {
                return TryMakeVeryHardQuestion(verse, out question);
            }

            List<string> candidates = ExtractCandidateWords(verse.Text)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (CurrentDifficulty == DIFFICULTY_HARD)
            {
                candidates = candidates
                    .Where(word => IsHardDifficultyAnswerCandidate(word) && CanMakeJosaVariants(word))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
            }

            int requestedBlankCount = GetBlankCount();
            int blankCount = Math.Min(requestedBlankCount, candidates.Count);

            if (blankCount <= 0)
            {
                return false;
            }

            List<string> selectedAnswers = SelectAnswersByDifficulty(candidates, blankCount);
            if (selectedAnswers.Count != blankCount)
            {
                return false;
            }

            List<string> orderedAnswers = OrderAnswersByAppearance(verse.Text, selectedAnswers);
            if (orderedAnswers.Count != blankCount)
            {
                return false;
            }

            if (!TryBuildClozeText(verse.Text, orderedAnswers, out string clozeText))
            {
                return false;
            }

            List<IReadOnlyList<string>> choiceSets = new List<IReadOnlyList<string>>();

            foreach (string answer in orderedAnswers)
            {
                List<string> choices = BuildChoices(answer);

                if (CurrentDifficulty == DIFFICULTY_EASY || CurrentDifficulty == DIFFICULTY_NORMAL)
                {
                    choices = RebuildEasyChoices(answer, choices);
                }

                if (choices.Count < GetChoiceCount())
                {
                    return false;
                }

                choiceSets.Add(choices);
            }

            question = new ClozeQuestion
            {
                Reference = verse.Ref,
                OriginalReference = verse.Ref,
                OriginalText = verse.Text,
                ClozeText = clozeText,
                Answers = orderedAnswers,
                ChoiceSets = choiceSets
            };

            return true;
        }

        private bool TryMakeSamuelRank1Question(VerseItem verse, out ClozeQuestion? question)
        {
            question = null;

            if (string.IsNullOrWhiteSpace(verse.Text))
            {
                return false;
            }

            question = new ClozeQuestion
            {
                Reference = verse.Ref,
                OriginalReference = verse.Ref,
                OriginalText = verse.Text,
                ClozeText = string.Empty,
                Answers = Array.Empty<string>(),
                ChoiceSets = Array.Empty<IReadOnlyList<string>>()
            };

            return true;
        }

        private bool CanMakeJosaVariants(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return false;
            }

            string normalizedWord = word.Trim();

            if (JosaVariantBlockedWords.Contains(normalizedWord))
            {
                return false;
            }

            return true;
        }

        private static List<string> OrderAnswersByAppearance(string text, IReadOnlyList<string> answers)
        {
            List<(string Answer, int Index)> indexedAnswers = new List<(string Answer, int Index)>();

            foreach (string answer in answers)
            {
                int index = text.IndexOf(answer, StringComparison.Ordinal);
                if (index < 0)
                {
                    return new List<string>();
                }

                indexedAnswers.Add((answer, index));
            }

            return indexedAnswers
                .OrderBy(x => x.Index)
                .Select(x => x.Answer)
                .ToList();
        }

        private bool TryBuildClozeText(string text, IReadOnlyList<string> answers, out string clozeText)
        {
            clozeText = text;

            List<ReplacementTarget> targets = new List<ReplacementTarget>();

            for (int i = 0; i < answers.Count; i++)
            {
                string answer = answers[i];
                int index = text.IndexOf(answer, StringComparison.Ordinal);

                if (index < 0)
                {
                    clozeText = text;
                    return false;
                }

                targets.Add(new ReplacementTarget(index, answer, i + 1));
            }

            if (targets.Select(x => x.Index).Distinct().Count() != targets.Count)
            {
                clozeText = text;
                return false;
            }

            string result = text;

            foreach (ReplacementTarget target in targets.OrderByDescending(x => x.Index))
            {
                string numberedBlank = $"[{target.Order}] ____";

                result = result.Substring(0, target.Index)
                       + numberedBlank
                       + result.Substring(target.Index + target.Answer.Length);
            }

            if (string.Equals(result, text, StringComparison.Ordinal))
            {
                clozeText = text;
                return false;
            }

            clozeText = result;
            return true;
        }

        private void PopulateChoiceGroups(ClozeQuestion question)
        {
            ChoiceGroups.Clear();

            for (int i = 0; i < question.ChoiceSets.Count; i++)
            {
                ClozeChoiceGroupItem group = new ClozeChoiceGroupItem
                {
                    BlankIndex = i
                };

                foreach (string choice in question.ChoiceSets[i])
                {
                    group.Choices.Add(choice);
                }

                ChoiceGroups.Add(group);
            }

            OnPropertyChanged(nameof(ChoiceGroups));
            OnPropertyChanged(nameof(AreChoiceGroupsVisible));
        }

        private void LoadQuestion(int index)
        {
            StopTimer();

            _current = _round[index];
            _tryLeft = GetTryCount();
            _isCorrect = false;
            _hintUsed = false;
            _isPreviewing = false;
            _isAutoNextScheduled = false;

            SelectedFirstChoice = null;
            SelectedSecondChoice = null;

            SamuelRank1InputText = string.Empty;
            ClearAllChoiceCollections();
            ClearVeryHardInputs();

            if (IsSamuelRank1Mode)
            {
                ReferenceText = _current.OriginalReference;
                QuestionText = string.Empty;
                FeedbackText = "권/장/절만 보고 말씀 전체를 입력하세요.";
            }
            else if (IsVeryHardDifficulty())
            {
                QuestionText = _current.ClozeText;
                ReferenceText = _current.Reference;
                InitializeVeryHardInputs(_current);
                FeedbackText = BuildInitialGuideText();
            }
            else
            {
                ReferenceText = _current.OriginalReference;

                PopulateChoiceGroups(_current);

                if (_current.IsDualBlank)
                {
                    foreach (string choice in _current.ChoiceSets[0])
                    {
                        FirstChoices.Add(choice);
                    }

                    foreach (string choice in _current.ChoiceSets[1])
                    {
                        SecondChoices.Add(choice);
                    }
                }
                else
                {
                    foreach (string choice in _current.ChoiceSets[0])
                    {
                        Choices.Add(choice);
                    }
                }

                QuestionText = _current.ClozeText;
                FeedbackText = BuildInitialGuideText();
            }

            if (IsTimeAttack)
            {
                _timeLeftSec = GetTimeAttackSeconds();
                _timer.Start();
            }
            else
            {
                _timeLeftSec = 0;
            }

            RaiseUiComputed();
        }

        private void OnTimerTick()
        {
            if (!IsTimeAttack || _isPreviewing)
            {
                return;
            }

            if (_isCorrect || _isRoundCompleted)
            {
                StopTimer();
                return;
            }

            _timeLeftSec = Math.Max(0, _timeLeftSec - 1);
            OnPropertyChanged(nameof(TimeLeftText));

            if (_timeLeftSec <= 0)
            {
                _isCorrect = false;
                _tryLeft = 0;

                if (_current != null)
                {
                    if (IsSamuelRank1Mode)
                    {
                        FeedbackText = $"시간 종료. 정답은 \"{_current.OriginalText}\" 입니다.";
                    }
                    else if (IsVeryHardDifficulty())
                    {
                        FeedbackText = $"시간 종료. 정답은 {BuildVeryHardAnswerSummary(_current)} 입니다.";
                    }
                    else
                    {
                        FeedbackText = $"시간 종료. 정답은 \"{string.Join(", ", _current.Answers)}\" 입니다.";
                    }
                }

                StopTimer();
                RaiseUiComputed();
                ScheduleAutoNext();
            }
        }

        private void StopTimer()
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
            }
        }

        private IReadOnlyList<Verse> BuildVerseList(int course, string selectedDay)
        {
            if (selectedDay == ALL_DAY_TEXT)
            {
                List<Verse> all = new List<Verse>();

                for (int day = 1; day <= VerseCatalog.MAX_DAY; day++)
                {
                    IReadOnlyList<Verse> list = VerseCatalog.GetAccumulated(course, day);
                    all.AddRange(list);
                }

                return all
                    .GroupBy(v => v.Ref)
                    .Select(g => g.First())
                    .ToList();
            }

            int oneDay = ParseDayNo(selectedDay);
            return VerseCatalog.GetAccumulated(course, oneDay).ToList();
        }

        private static int ParseCourseNo(string courseText)
        {
            string digits = new string(courseText.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out int number) ? number : 1;
        }

        private static int ParseDayNo(string dayText)
        {
            string digits = new string(dayText.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out int number) ? number : 1;
        }

        private static IEnumerable<string> ExtractCandidateWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                yield break;
            }

            string[] raw = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < raw.Length; i++)
            {
                string word = raw[i].Trim();
                word = TrimPunctuation(word);

                if (word.Length < 3)
                {
                    continue;
                }

                if (word.All(char.IsDigit))
                {
                    continue;
                }

                yield return word;
            }
        }

        private static string TrimPunctuation(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            return Regex.Replace(s, @"^\p{P}+|\p{P}+$", "");
        }

        private static string ReplacePlaceholderByOrder(string text, int placeholderOrder, string replacement)
        {
            const string placeholder = "____";
            int searchStart = 0;

            for (int i = 0; i <= placeholderOrder; i++)
            {
                int idx = text.IndexOf(placeholder, searchStart, StringComparison.Ordinal);

                if (idx < 0)
                {
                    return text;
                }

                if (i == placeholderOrder)
                {
                    return text.Substring(0, idx) + replacement + text.Substring(idx + placeholder.Length);
                }

                searchStart = idx + placeholder.Length;
            }

            return text;
        }

        private void ClearAllChoiceCollections()
        {
            Choices.Clear();
            FirstChoices.Clear();
            SecondChoices.Clear();
            ChoiceGroups.Clear();
        }

        private void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private void RaiseUiComputed()
        {
            OnPropertyChanged(nameof(ProgressText));
            OnPropertyChanged(nameof(ScoreText));
            OnPropertyChanged(nameof(ComboText));
            OnPropertyChanged(nameof(TimeLeftText));
            OnPropertyChanged(nameof(DifficultyText));
            OnPropertyChanged(nameof(IsNormalMode));

            OnPropertyChanged(nameof(AreSingleChoicesVisible));
            OnPropertyChanged(nameof(AreDualChoicesVisible));
            OnPropertyChanged(nameof(AreChoiceGroupsVisible));
            OnPropertyChanged(nameof(IsVeryHardInputVisible));
            OnPropertyChanged(nameof(IsSamuelRank1InputVisible));

            OnPropertyChanged(nameof(AreChoicesEnabled));
            OnPropertyChanged(nameof(AreFirstChoicesEnabled));
            OnPropertyChanged(nameof(AreSecondChoicesEnabled));
            OnPropertyChanged(nameof(IsHintEnabled));
            OnPropertyChanged(nameof(CanSubmitVeryHardAnswer));
            OnPropertyChanged(nameof(CanSubmitSamuelRank1Answer));

            try
            {
                CommandManager.InvalidateRequerySuggested();
            }
            catch
            {
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private sealed class ClozeQuestion
        {
            public string Reference { get; init; } = string.Empty;
            public string OriginalReference { get; init; } = string.Empty;
            public string OriginalText { get; init; } = string.Empty;
            public string ClozeText { get; init; } = string.Empty;
            public IReadOnlyList<string> Answers { get; init; } = Array.Empty<string>();
            public IReadOnlyList<IReadOnlyList<string>> ChoiceSets { get; init; } = Array.Empty<IReadOnlyList<string>>();
            public bool IsDualBlank => Answers.Count == 2;
        }

        private sealed class ReplacementTarget
        {
            public int Index { get; }
            public string Answer { get; }
            public int Order { get; }

            public ReplacementTarget(int index, string answer, int order)
            {
                Index = index;
                Answer = answer;
                Order = order;
            }
        }
    }
}