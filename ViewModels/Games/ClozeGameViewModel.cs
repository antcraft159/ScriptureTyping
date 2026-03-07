using ScriptureTyping.Commands;
using ScriptureTyping.Data;
using ScriptureTyping.Services;
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
        private const int DEFAULT_ROUND_COUNT = 10;

        private const string DIFFICULTY_EASY = "쉬움";
        private const string DIFFICULTY_NORMAL = "보통";
        private const string DIFFICULTY_HARD = "어려움";
        private const string DIFFICULTY_VERY_HARD = "매우 어려움";
        private const string DIFFICULTY_SAMUEL_RANK1 = "사무엘 1등";

        private static readonly TimeSpan AUTO_NEXT_DELAY = TimeSpan.FromMilliseconds(2000);
        private static readonly TimeSpan SAMUEL_PREVIEW_DELAY = TimeSpan.FromMilliseconds(1200);

        private static readonly string[] KnownJosa =
        {
            "으로부터", "에게서", "께서는", "에서는", "으로는", "으로도",
            "이었다", "이니라", "이라는", "이라도",
            "으로", "에게", "께서", "에서", "이다",
            "이나", "나마", "처럼", "같이",
            "이", "가", "은", "는", "을", "를",
            "와", "과", "도", "만", "의", "로", "에", "께"
        };

        private static readonly string[] EndingPatterns =
        {
            "하였느니라", "하였더라", "하였도다",
            "되었느니라", "되었더라", "되었도다",
            "하시느니라", "하시니라", "하느니라", "하더라", "하도다", "하노라",
            "되느니라", "되니라", "되리라", "되더라", "되도다",
            "느니라", "이니라", "이로다",
            "으리라", "리라", "니라", "더라", "도다", "노라", "이라",
            "하라", "하여라", "하니", "하되", "하고",
            "이다", "이니", "이며"
        };

        private static readonly string[] ReplacementEndings =
        {
            "느니라", "이니라", "이로다",
            "으리라", "리라", "니라", "더라", "도다", "노라", "이라",
            "하라", "하여라", "하니", "하되", "하고",
            "이다", "이니", "이며"
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

        private string? _selectedFirstChoice;
        private string? _selectedSecondChoice;

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
        public bool IsNormalMode => CurrentDifficulty == DIFFICULTY_NORMAL;

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

        public bool AreSingleChoicesVisible => _current != null && !_current.IsDualBlank && Choices.Count > 0;

        public bool AreDualChoicesVisible =>
            _current != null &&
            _current.IsDualBlank &&
            FirstChoices.Count > 0 &&
            SecondChoices.Count > 0;

        public bool AreChoicesEnabled =>
            CanAnswerChoices() &&
            _current != null &&
            !_current.IsDualBlank &&
            Choices.Count > 0;

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

        public bool IsHintEnabled =>
            !_isRoundCompleted &&
            !_hintUsed &&
            !_isCorrect &&
            _tryLeft > 0 &&
            !_isPreviewing &&
            _current != null;

        public ICommand BackCommand { get; }
        public ICommand SelectChoiceCommand { get; }
        public ICommand SelectFirstChoiceCommand { get; }
        public ICommand SelectSecondChoiceCommand { get; }
        public ICommand HintCommand { get; }
        public ICommand RestartCommand { get; }

        private string CurrentDifficulty => string.IsNullOrWhiteSpace(SelectedDifficulty) ? DIFFICULTY_EASY : SelectedDifficulty!;

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

            QuestionText = "상단에서 과정/일차/모드를 선택 후 [적용]을 누르세요.";
            ReferenceText = string.Empty;
            FeedbackText = string.Empty;
            ClearAllChoiceCollections();
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
                QuestionText = "구절이 없습니다.";
                ReferenceText = string.Empty;
                FeedbackText = string.Empty;
                ClearAllChoiceCollections();
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

            IsTimeAttack = IsTimeAttackDifficulty(CurrentDifficulty);
            OnPropertyChanged(nameof(IsTimeAttack));
            OnPropertyChanged(nameof(DifficultyText));
            OnPropertyChanged(nameof(IsNormalMode));

            BuildWordPool(verses);
            BuildRound(verses);

            if (_round.Count <= 0)
            {
                QuestionText = "문제를 만들 수 있는 구절이 없습니다.";
                ReferenceText = string.Empty;
                FeedbackText = "다른 과정/일차를 선택해 주세요.";
                ClearAllChoiceCollections();
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
            return !_isRoundCompleted &&
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

                QuestionText = "라운드 종료!";
                ReferenceText = string.Empty;
                ClearAllChoiceCollections();
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

            int limit = Math.Min(DEFAULT_ROUND_COUNT, shuffled.Count);

            for (int i = 0; i < limit; i++)
            {
                VerseItem verse = shuffled[i];

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

            List<string> candidates = ExtractCandidateWords(verse.Text)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            int blankCount = GetBlankCount();

            if (candidates.Count < blankCount)
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

                if (choices.Count < Math.Min(3, GetChoiceCount()))
                {
                    return false;
                }

                choiceSets.Add(choices);
            }

            question = new ClozeQuestion
            {
                Reference = verse.Ref,
                OriginalText = verse.Text,
                ClozeText = clozeText,
                Answers = orderedAnswers,
                ChoiceSets = choiceSets
            };

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

        private bool TryBuildClozeText(string text, IReadOnlyList<string> answers, out string clozeText)
        {
            clozeText = text;

            List<ReplacementTarget> targets = new List<ReplacementTarget>();

            foreach (string answer in answers)
            {
                int index = text.IndexOf(answer, StringComparison.Ordinal);
                if (index < 0)
                {
                    clozeText = text;
                    return false;
                }

                targets.Add(new ReplacementTarget(index, answer));
            }

            if (targets.Select(x => x.Index).Distinct().Count() != targets.Count)
            {
                clozeText = text;
                return false;
            }

            string result = text;

            foreach (ReplacementTarget target in targets.OrderByDescending(x => x.Index))
            {
                result = result.Substring(0, target.Index) + "____" + result.Substring(target.Index + target.Answer.Length);
            }

            if (string.Equals(result, text, StringComparison.Ordinal))
            {
                clozeText = text;
                return false;
            }

            clozeText = result;
            return true;
        }

        private List<string> BuildChoices(string answer)
        {
            int choiceCount = GetChoiceCount();

            HashSet<string> set = new HashSet<string>(StringComparer.Ordinal)
            {
                answer
            };

            foreach (string distractor in GenerateDistractors(answer))
            {
                if (set.Count >= choiceCount)
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(distractor))
                {
                    continue;
                }

                if (string.Equals(distractor, answer, StringComparison.Ordinal))
                {
                    continue;
                }

                set.Add(distractor);
            }

            bool allowGlobalFallback =
                CurrentDifficulty == DIFFICULTY_EASY ||
                CurrentDifficulty == DIFFICULTY_NORMAL;

            if (allowGlobalFallback)
            {
                int safety = 0;

                while (set.Count < choiceCount && safety < 300)
                {
                    safety++;

                    if (_globalWordPool.Count <= 0)
                    {
                        break;
                    }

                    string word = _globalWordPool[_rng.Next(_globalWordPool.Count)];

                    if (string.IsNullOrWhiteSpace(word) ||
                        string.Equals(word, answer, StringComparison.Ordinal) ||
                        !IsValidChoiceWord(word))
                    {
                        continue;
                    }

                    set.Add(word);
                }
            }
            else
            {
                foreach (string extra in GenerateExtendedHardDistractors(answer))
                {
                    if (set.Count >= choiceCount)
                    {
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(extra) ||
                        string.Equals(extra, answer, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    set.Add(extra);
                }
            }

            List<string> list = set.ToList();
            Shuffle(list);
            return list.Take(choiceCount).ToList();
        }

        private IEnumerable<string> GenerateDistractors(string answer)
        {
            List<string> sameLengthWords = _globalWordPool
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => !string.Equals(x, answer, StringComparison.Ordinal))
                .Where(IsValidChoiceWord)
                .Where(x => Math.Abs(x.Length - answer.Length) <= 1)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (CurrentDifficulty == DIFFICULTY_EASY)
            {
                Shuffle(sameLengthWords);

                foreach (string item in sameLengthWords)
                {
                    yield return item;
                }

                yield break;
            }

            if (CurrentDifficulty == DIFFICULTY_NORMAL)
            {
                List<string> normalWords = sameLengthWords
                    .OrderBy(x => Math.Abs(x.Length - answer.Length))
                    .ThenByDescending(x => SharedPrefixScore(answer, x))
                    .ToList();

                foreach (string item in normalWords)
                {
                    yield return item;
                }

                yield break;
            }

            foreach (string variant in BuildSuffixVariants(answer))
            {
                yield return variant;
            }
        }

        private IEnumerable<string> GenerateExtendedHardDistractors(string answer)
        {
            HashSet<string> results = new HashSet<string>(StringComparer.Ordinal);

            foreach (string item in BuildEndingVariants(answer))
            {
                results.Add(item);
            }

            foreach (string item in BuildJosaLikeVariants(answer))
            {
                results.Add(item);
            }

            foreach (string item in BuildStemSimilarWords(answer))
            {
                results.Add(item);
            }

            foreach (string item in results)
            {
                yield return item;
            }
        }

        private IEnumerable<string> BuildSuffixVariants(string answer)
        {
            if (string.IsNullOrWhiteSpace(answer))
            {
                yield break;
            }

            string normalized = answer.Trim();
            string stem = RemoveKnownJosa(normalized, out string? originalJosa);

            if (!string.IsNullOrWhiteSpace(originalJosa) && !string.IsNullOrWhiteSpace(stem))
            {
                foreach (string variant in BuildJosaVariants(stem))
                {
                    if (!string.Equals(variant, normalized, StringComparison.Ordinal))
                    {
                        yield return variant;
                    }
                }

                yield break;
            }

            bool yielded = false;

            foreach (string variant in BuildEndingVariants(normalized))
            {
                if (!string.Equals(variant, normalized, StringComparison.Ordinal))
                {
                    yielded = true;
                    yield return variant;
                }
            }

            if (!yielded)
            {
                foreach (string word in BuildStemSimilarWords(normalized))
                {
                    if (!string.Equals(word, normalized, StringComparison.Ordinal))
                    {
                        yield return word;
                    }
                }
            }
        }

        private IEnumerable<string> BuildEndingVariants(string answer)
        {
            if (string.IsNullOrWhiteSpace(answer))
            {
                yield break;
            }

            HashSet<string> results = new HashSet<string>(StringComparer.Ordinal);
            string normalized = answer.Trim();

            foreach (string ending in EndingPatterns.OrderByDescending(x => x.Length))
            {
                if (normalized.Length > ending.Length && normalized.EndsWith(ending, StringComparison.Ordinal))
                {
                    string stem = normalized.Substring(0, normalized.Length - ending.Length);

                    foreach (string newEnding in ReplacementEndings)
                    {
                        string candidate = stem + newEnding;

                        if (!string.Equals(candidate, normalized, StringComparison.Ordinal) &&
                            IsValidChoiceWord(candidate))
                        {
                            results.Add(candidate);
                        }
                    }

                    break;
                }
            }

            foreach (string item in results)
            {
                yield return item;
            }
        }

        private IEnumerable<string> BuildJosaLikeVariants(string answer)
        {
            if (string.IsNullOrWhiteSpace(answer))
            {
                yield break;
            }

            HashSet<string> results = new HashSet<string>(StringComparer.Ordinal);
            string normalized = answer.Trim();
            string stem = RemoveKnownJosa(normalized, out string? originalJosa);

            if (!string.IsNullOrWhiteSpace(originalJosa) && !string.IsNullOrWhiteSpace(stem))
            {
                foreach (string item in BuildJosaVariants(stem))
                {
                    results.Add(item);
                }
            }
            else if (normalized.Length >= 2 && IsMostlyNounLike(normalized))
            {
                bool hasBatchim = HasFinalConsonant(normalized);

                results.Add(normalized + (hasBatchim ? "이" : "가"));
                results.Add(normalized + (hasBatchim ? "은" : "는"));
                results.Add(normalized + (hasBatchim ? "을" : "를"));
                results.Add(normalized + (hasBatchim ? "과" : "와"));
                results.Add(normalized + "의");
                results.Add(normalized + "도");
                results.Add(normalized + "만");
            }

            foreach (string item in results)
            {
                if (!string.Equals(item, normalized, StringComparison.Ordinal) &&
                    IsValidChoiceWord(item))
                {
                    yield return item;
                }
            }
        }

        private IEnumerable<string> BuildJosaVariants(string stem)
        {
            bool hasBatchim = HasFinalConsonant(stem);

            List<string> variants = new List<string>
            {
                stem + (hasBatchim ? "이" : "가"),
                stem + (hasBatchim ? "은" : "는"),
                stem + (hasBatchim ? "을" : "를"),
                stem + (hasBatchim ? "과" : "와"),
                stem + "도",
                stem + "만",
                stem + "의",
                stem + (hasBatchim ? "으로" : "로"),
                stem + "에",
                stem + (hasBatchim ? "이나" : "나")
            };

            Shuffle(variants);

            foreach (string item in variants.Distinct(StringComparer.Ordinal))
            {
                yield return item;
            }
        }

        private IEnumerable<string> BuildStemSimilarWords(string answer)
        {
            List<string> similarWords = _globalWordPool
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => !string.Equals(x, answer, StringComparison.Ordinal))
                .Where(IsValidChoiceWord)
                .Where(x => Math.Abs(x.Length - answer.Length) <= 1)
                .Where(x => SharedPrefixScore(answer, x) >= Math.Max(2, answer.Length / 2))
                .Distinct(StringComparer.Ordinal)
                .OrderByDescending(x => SharedPrefixScore(answer, x))
                .ThenBy(x => Math.Abs(x.Length - answer.Length))
                .ToList();

            foreach (string item in similarWords)
            {
                yield return item;
            }
        }

        private static bool IsValidChoiceWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return false;
            }

            string normalized = word.Trim();

            if (normalized.Length < 2)
            {
                return false;
            }

            if (normalized.Any(char.IsWhiteSpace))
            {
                return false;
            }

            int koreanCount = normalized.Count(ch => ch >= 0xAC00 && ch <= 0xD7A3);
            if (koreanCount < Math.Max(1, normalized.Length / 2))
            {
                return false;
            }

            return true;
        }

        private static bool IsMostlyNounLike(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return false;
            }

            string[] verbLikeEndings =
            {
                "하다", "되다", "가다", "오다", "보다", "이다",
                "니라", "리라", "더라", "도다", "노라",
                "하라", "하여라", "하며", "하고", "하니", "하되"
            };

            return !verbLikeEndings.Any(word.EndsWith);
        }

        private static string RemoveKnownJosa(string word, out string? josa)
        {
            foreach (string candidate in KnownJosa.OrderByDescending(x => x.Length))
            {
                if (word.Length > candidate.Length && word.EndsWith(candidate, StringComparison.Ordinal))
                {
                    josa = candidate;
                    return word.Substring(0, word.Length - candidate.Length);
                }
            }

            josa = null;
            return word;
        }

        private static bool HasFinalConsonant(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            char lastChar = text[^1];

            if (lastChar < 0xAC00 || lastChar > 0xD7A3)
            {
                return false;
            }

            int code = lastChar - 0xAC00;
            int jong = code % 28;
            return jong != 0;
        }

        private int SharedPrefixScore(string a, string b)
        {
            int len = Math.Min(a.Length, b.Length);
            int count = 0;

            for (int i = 0; i < len; i++)
            {
                if (a[i] != b[i])
                {
                    break;
                }

                count++;
            }

            return count;
        }

        private async void LoadQuestion(int index)
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

            ReferenceText = _current.Reference;
            ClearAllChoiceCollections();

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

            _questionVersion++;
            int localVersion = _questionVersion;

            if (CurrentDifficulty == DIFFICULTY_SAMUEL_RANK1)
            {
                _isPreviewing = true;
                QuestionText = _current.OriginalText;
                FeedbackText = "잠깐 보여줍니다. 집중해서 기억하세요.";
                _timeLeftSec = 0;
                RaiseUiComputed();

                await Task.Delay(SAMUEL_PREVIEW_DELAY);

                if (localVersion != _questionVersion || _current == null || _isRoundCompleted)
                {
                    return;
                }

                _isPreviewing = false;
                QuestionText = _current.ClozeText;
                FeedbackText = BuildInitialGuideText();
            }
            else
            {
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
                    FeedbackText = $"시간 종료. 정답은 \"{string.Join(", ", _current.Answers)}\" 입니다.";
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
        }

        private void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
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
                DIFFICULTY_VERY_HARD => 12,
                DIFFICULTY_SAMUEL_RANK1 => 8,
                _ => 15
            };
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

            OnPropertyChanged(nameof(AreChoicesEnabled));
            OnPropertyChanged(nameof(AreFirstChoicesEnabled));
            OnPropertyChanged(nameof(AreSecondChoicesEnabled));
            OnPropertyChanged(nameof(IsHintEnabled));

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
            public string OriginalText { get; init; } = string.Empty;
            public string ClozeText { get; init; } = string.Empty;
            public IReadOnlyList<string> Answers { get; init; } = Array.Empty<string>();
            public IReadOnlyList<IReadOnlyList<string>> ChoiceSets { get; init; } = Array.Empty<IReadOnlyList<string>>();
            public bool IsDualBlank => Answers.Count >= 2;
        }

        private readonly struct ReplacementTarget
        {
            public ReplacementTarget(int index, string answer)
            {
                Index = index;
                Answer = answer;
            }

            public int Index { get; }
            public string Answer { get; }
        }
    }
}