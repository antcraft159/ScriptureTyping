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
    public sealed class ClozeGameViewModel : INotifyPropertyChanged
    {
        private const string DEFAULT_TITLE = "빈칸 채우기";
        private const string ALL_DAY_TEXT = "전일차";
        private const int ALL_DAY_INDEX = 0;

        private const int DEFAULT_ROUND_COUNT = 10;
        private const int DEFAULT_TRY_LEFT = 2;
        private const int DEFAULT_CHOICE_COUNT = 6;
        private const int TIME_ATTACK_SECONDS = 15;

        // 자동 다음문제 이동 딜레이(피드백 보여주기 용)
        private static readonly TimeSpan AUTO_NEXT_DELAY = TimeSpan.FromMilliseconds(2000);

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
        private bool _isAnswered;
        private bool _isRoundCompleted;

        private int _timeLeftSec;

        private string _title = DEFAULT_TITLE;
        private string _questionText = string.Empty;
        private string _referenceText = string.Empty;
        private string _feedbackText = string.Empty;

        private string? _selectedCourse;
        private string? _selectedDay;
        private string _selectionError = string.Empty;

        private ClozeQuestion? _current;

        // 자동 다음 문제 예약 중복 방지
        private bool _isAutoNextScheduled;

        public event PropertyChangedEventHandler? PropertyChanged;

        public event Action? BackRequested;

        // -----------------------------
        //  생성자
        // -----------------------------

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
            HintCommand = new RelayCommand(_ => Hint(), _ => IsHintEnabled);
            RestartCommand = new RelayCommand(_ => Restart(), _ => true);

            //  게임 상단 선택 UI 커맨드
            ApplySelectionCommand = new RelayCommand(_ => ApplySelection(), _ => CanApplySelection());

            Title = DEFAULT_TITLE;

            InitSelectionUi();
            ApplySelectionFromContextOrDefault(); // 시작 시 한번 세팅
        }

        // -----------------------------
        //  상단 선택 UI 바인딩
        // -----------------------------

        public ObservableCollection<string> Courses { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Days { get; } = new ObservableCollection<string>();

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
        }

        private void ApplySelectionFromContextOrDefault()
        {
            // Context가 있으면 그걸 UI에 반영
            if (_ctx.HasSelection)
            {
                SelectedCourse = _ctx.SelectedCourseId;
                int dayIndex = _ctx.SelectedDayIndex ?? 0;
                SelectedDay = dayIndex == 0 ? ALL_DAY_TEXT : $"{dayIndex}일차";

                ApplySelection();
                return;
            }

            // 없으면 기본값(1과정/전일차 같은)만 잡고, 아직 게임 시작은 안 함
            SelectedCourse = Courses.FirstOrDefault();
            SelectedDay = ALL_DAY_TEXT;

            QuestionText = "상단에서 과정/일차 선택 후 [적용]을 누르세요.";
            ReferenceText = "";
            FeedbackText = "";
            Choices.Clear();
            RaiseUiComputed();
        }

        private bool CanApplySelection()
        {
            return !string.IsNullOrWhiteSpace(SelectedCourse) && !string.IsNullOrWhiteSpace(SelectedDay);
        }

        private void ApplySelection()
        {
            if (string.IsNullOrWhiteSpace(SelectedCourse) || string.IsNullOrWhiteSpace(SelectedDay))
            {
                SelectionError = "과정/일차를 선택해 주세요.";
                return;
            }

            int courseNo = ParseCourseNo(SelectedCourse);
            int dayIndex = (SelectedDay == ALL_DAY_TEXT) ? ALL_DAY_INDEX : ParseDayNo(SelectedDay);

            // VerseCatalog로 구절 로드
            IReadOnlyList<Verse> verses = BuildVerseList(courseNo, SelectedDay);

            if (verses.Count == 0)
            {
                SelectionError = "해당 선택에 구절이 없습니다.";
                QuestionText = "구절이 없습니다.";
                ReferenceText = "";
                FeedbackText = "";
                Choices.Clear();
                RaiseUiComputed();
                return;
            }

            // SelectionContext에 저장(게임/다른 곳도 동기화)
            List<VerseItem> items = verses.Select(v => new VerseItem { Ref = v.Ref, Text = v.Text }).ToList();
            _ctx.SetSelection(SelectedCourse, dayIndex, items);

            // 게임 라운드 재생성(점수/인덱스 리셋)
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

            _tryLeft = DEFAULT_TRY_LEFT;
            _isCorrect = false;
            _hintUsed = false;
            _isAnswered = false;
            _isRoundCompleted = false;

            _isAutoNextScheduled = false;

            BuildWordPool(verses);
            BuildRound(verses);

            if (_round.Count <= 0)
            {
                QuestionText = "문제를 만들 수 있는 구절이 없습니다.";
                ReferenceText = "";
                FeedbackText = "다른 과정/일차를 선택해 주세요.";
                Choices.Clear();
                RaiseUiComputed();
                return;
            }

            LoadQuestion(0);
        }

        // -----------------------------
        // 기존 게임 바인딩
        // -----------------------------

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

        public bool IsTimeAttack { get; private set; } = false;
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

        public ObservableCollection<string> Choices { get; } = new ObservableCollection<string>();

        public bool AreChoicesEnabled => !_isRoundCompleted && !_isCorrect && _tryLeft > 0;
        public bool IsHintEnabled => !_isRoundCompleted && !_hintUsed && !_isCorrect && _tryLeft > 0 && _current != null;

        // -----------------------------
        // Commands
        // -----------------------------

        public ICommand BackCommand { get; }
        public ICommand SelectChoiceCommand { get; }
        public ICommand HintCommand { get; }
        public ICommand RestartCommand { get; }

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

        private void SelectChoice(string? choice)
        {
            if (choice == null) return;
            if (_current == null) return;
            if (!AreChoicesEnabled) return;

            _isAnswered = true;

            if (string.Equals(choice.Trim(), _current.Answer.Trim(), StringComparison.Ordinal))
            {
                _isCorrect = true;
                _score += 10;
                _combo += 1;

                FeedbackText = "정답!";
                StopTimer();
                RaiseUiComputed();

                // 다음 버튼이 없으니 정답도 자동으로 넘어가게 처리
                ScheduleAutoNext();
                return;
            }

            _isCorrect = false;
            _tryLeft -= 1;
            _score = Math.Max(0, _score - 2);
            _combo = 0;

            if (_tryLeft <= 0)
            {
                FeedbackText = $"기회 소진. 정답은 \"{_current.Answer}\" 입니다.";
                StopTimer();
                RaiseUiComputed();

                // ✅ 요청: 기회 0이면 자동으로 다음 문제
                ScheduleAutoNext();
                return;
            }

            FeedbackText = $"틀렸습니다. 다시 선택하세요. (기회 {_tryLeft}회)";
            RaiseUiComputed();
        }

        private async void ScheduleAutoNext()
        {
            if (_isAutoNextScheduled) return;
            _isAutoNextScheduled = true;

            try
            {
                await Task.Delay(AUTO_NEXT_DELAY);
                AutoNext();
            }
            finally
            {
                // LoadQuestion에서 다시 false로 내려줘도 되지만,
                // 혹시 라운드 종료에서 멈춘 경우 대비해서 여기서도 한번 관리
                if (_isRoundCompleted) _isAutoNextScheduled = false;
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
                ReferenceText = "";
                Choices.Clear();

                FeedbackText = $"총 {_round.Count}문제 | 점수 {_score}";
                _isAutoNextScheduled = false;

                RaiseUiComputed();
                return;
            }

            LoadQuestion(_index);
        }

        private void Hint()
        {
            if (!IsHintEnabled) return;
            if (_current == null) return;

            _hintUsed = true;
            _score = Math.Max(0, _score - 1);

            string first = string.IsNullOrWhiteSpace(_current.Answer) ? "" : _current.Answer.Substring(0, 1);
            QuestionText = _current.ClozeText.Replace("____", $"{first}…");

            FeedbackText = "힌트 사용(-1점).";
            RaiseUiComputed();
        }

        private void Restart()
        {
            // 현재 선택(상단 콤보)을 그대로 기준으로 다시 적용
            ApplySelection();
        }

        // -----------------------------
        // Round Build
        // -----------------------------

        private void BuildWordPool(IEnumerable<VerseItem> verses)
        {
            _globalWordPool.Clear();

            foreach (VerseItem v in verses)
            {
                foreach (string w in ExtractCandidateWords(v.Text))
                {
                    _globalWordPool.Add(w);
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
                VerseItem v = shuffled[i];

                if (!TryMakeQuestion(v, out ClozeQuestion? q) || q == null)
                {
                    continue;
                }

                _round.Add(q);
            }

            Shuffle(_round);
        }

        private bool TryMakeQuestion(VerseItem verse, out ClozeQuestion? question)
        {
            question = null;

            List<string> candidates = ExtractCandidateWords(verse.Text).ToList();
            if (candidates.Count <= 0) return false;

            string answer = candidates[_rng.Next(candidates.Count)];
            string cloze = ReplaceFirst(verse.Text, answer, "____");
            if (string.Equals(cloze, verse.Text, StringComparison.Ordinal)) return false;

            List<string> choices = BuildChoices(answer);

            question = new ClozeQuestion
            {
                Reference = verse.Ref,
                ClozeText = cloze,
                Answer = answer,
                Choices = choices
            };

            return true;
        }

        private List<string> BuildChoices(string answer)
        {
            HashSet<string> set = new HashSet<string>(StringComparer.Ordinal) { answer };

            int safety = 0;
            while (set.Count < DEFAULT_CHOICE_COUNT && safety < 5000)
            {
                safety++;
                if (_globalWordPool.Count <= 0) break;

                string w = _globalWordPool[_rng.Next(_globalWordPool.Count)];
                if (string.IsNullOrWhiteSpace(w)) continue;
                if (string.Equals(w, answer, StringComparison.Ordinal)) continue;

                set.Add(w);
            }

            while (set.Count < DEFAULT_CHOICE_COUNT)
            {
                set.Add("...");
            }

            List<string> list = set.ToList();
            Shuffle(list);
            return list;
        }

        private void LoadQuestion(int index)
        {
            StopTimer();

            _current = _round[index];

            _tryLeft = DEFAULT_TRY_LEFT;
            _isCorrect = false;
            _hintUsed = false;
            _isAnswered = false;

            _isAutoNextScheduled = false;

            QuestionText = _current.ClozeText;
            ReferenceText = _current.Reference;

            Choices.Clear();
            foreach (string c in _current.Choices)
            {
                Choices.Add(c);
            }

            FeedbackText = $"보기에서 정답을 고르세요. (기회 {_tryLeft}회)";

            if (IsTimeAttack)
            {
                _timeLeftSec = TIME_ATTACK_SECONDS;
                _timer.Start();
            }
            else
            {
                _timeLeftSec = 0;
            }

            RaiseUiComputed();
        }

        // -----------------------------
        // Timer
        // -----------------------------

        private void OnTimerTick()
        {
            if (!IsTimeAttack) return;
            if (_isCorrect) { StopTimer(); return; }
            if (_isRoundCompleted) { StopTimer(); return; }

            _timeLeftSec = Math.Max(0, _timeLeftSec - 1);
            OnPropertyChanged(nameof(TimeLeftText));

            if (_timeLeftSec <= 0)
            {
                _isAnswered = true;
                _isCorrect = false;
                _tryLeft = 0;

                if (_current != null)
                {
                    FeedbackText = $"시간 종료. 정답은 \"{_current.Answer}\" 입니다.";
                }

                StopTimer();
                RaiseUiComputed();

                // 타임아웃도 기회 0 처리이니 자동 다음
                ScheduleAutoNext();
            }
        }

        private void StopTimer()
        {
            if (_timer.IsEnabled) _timer.Stop();
        }

        // -----------------------------
        // Verse loading (CourseSelect과 동일 로직)
        // -----------------------------

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

                return all.GroupBy(v => v.Ref).Select(g => g.First()).ToList();
            }

            int oneDay = ParseDayNo(selectedDay);
            return VerseCatalog.GetAccumulated(course, oneDay).ToList();
        }

        private static int ParseCourseNo(string courseText)
        {
            string digits = new string(courseText.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out int n) ? n : 1;
        }

        private static int ParseDayNo(string dayText)
        {
            string digits = new string(dayText.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out int n) ? n : 1;
        }

        // -----------------------------
        // Helpers
        // -----------------------------

        private static IEnumerable<string> ExtractCandidateWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) yield break;

            string[] raw = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < raw.Length; i++)
            {
                string w = raw[i].Trim();
                w = TrimPunctuation(w);

                if (w.Length < 3) continue;
                if (w.All(char.IsDigit)) continue;

                yield return w;
            }
        }

        private static string TrimPunctuation(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return Regex.Replace(s, @"^\p{P}+|\p{P}+$", "");
        }

        private static string ReplaceFirst(string text, string search, string replace)
        {
            int idx = text.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return text;

            return text.Substring(0, idx) + replace + text.Substring(idx + search.Length);
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

            OnPropertyChanged(nameof(AreChoicesEnabled));
            OnPropertyChanged(nameof(IsHintEnabled));

            try { CommandManager.InvalidateRequerySuggested(); } catch { }
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private sealed class ClozeQuestion
        {
            public string Reference { get; init; } = string.Empty;
            public string ClozeText { get; init; } = string.Empty;
            public string Answer { get; init; } = string.Empty;
            public IReadOnlyList<string> Choices { get; init; } = Array.Empty<string>();
        }
    }
}