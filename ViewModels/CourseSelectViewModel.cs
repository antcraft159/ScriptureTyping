// 파일명: ViewModels/CourseSelectViewModel.cs
using ScriptureTyping.Commands;
using ScriptureTyping.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ScriptureTyping.ViewModels
{
    public sealed class CourseTypingResultItem
    {
        public string Ref { get; init; } = string.Empty;
        public string Expected { get; init; } = string.Empty;
        public string Typed { get; init; } = string.Empty;
        public bool IsCorrect { get; init; }
    }

    public sealed class CourseSelectViewModel : INotifyPropertyChanged
    {
        private const string ALL_DAY_TEXT = "전일차";
        private const string DEFAULT_GUIDE_TEXT = "과정/일차 선택 후 학습 시작을 누르세요.";
        private const string EMPTY_SELECTION_TEXT = "과정/일차를 선택해 주세요.";
        private const string EMPTY_COURSE_TEXT = "과정을 선택해 주세요.";
        private const string EMPTY_DAY_TEXT = "일차를 선택해 주세요.";
        private const string NO_VERSE_TEXT = "해당 선택에 구절이 없습니다.";
        private const string COMPLETION_POPUP_TEXT = "적으신 암송은 끝났습니다";
        private const int COMPLETION_POPUP_MILLISECONDS = 3000;

        private readonly Action<object> _navigate;
        private readonly Random _rng = new Random();

        private List<Verse> _verses = new List<Verse>();
        private Queue<Verse> _queue = new Queue<Verse>();
        private Verse? _currentVerse;

        private bool _useAccumulated = true;
        private string? _selectedCourse;
        private string? _selectedDay;
        private string? _courseErrorMessage;

        private string _currentVerseRef = string.Empty;
        private string _currentVerseText = DEFAULT_GUIDE_TEXT;
        private string _userTypedText = string.Empty;

        private bool _isTypingLocked = true;
        private bool _isSetCompleted;

        private bool _isCompletionPopupVisible;
        private string _completionPopupText = COMPLETION_POPUP_TEXT;

        private int _correctCount;
        private int _wrongCount;
        private int _totalCount;

        private bool _isStatsVisible;
        private string _statsSummary = string.Empty;

        public ObservableCollection<string> Courses { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Days { get; } = new ObservableCollection<string>();

        public string AllDay => ALL_DAY_TEXT;

        public ObservableCollection<CourseTypingResultItem> Results { get; } = new ObservableCollection<CourseTypingResultItem>();

        public ICommand StartLearningCommand { get; }
        public ICommand NextVerseCommand { get; }

        // ✅ 추가: XAML의 Typing.* 바인딩을 살리기 위한 브릿지
        public CourseSelectViewModel Typing => this;

        public bool UseAccumulated
        {
            get => _useAccumulated;
            set
            {
                if (_useAccumulated == value) return;
                _useAccumulated = value;
                OnPropertyChanged();
            }
        }

        public string? SelectedCourse
        {
            get => _selectedCourse;
            set
            {
                if (_selectedCourse == value) return;
                _selectedCourse = value;
                OnPropertyChanged();
                ValidateSelection(clearOnly: true);
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
                ValidateSelection(clearOnly: true);
            }
        }

        public string? CourseErrorMessage
        {
            get => _courseErrorMessage;
            set
            {
                if (_courseErrorMessage == value) return;
                _courseErrorMessage = value;
                OnPropertyChanged();
            }
        }

        public string CurrentVerseRef
        {
            get => _currentVerseRef;
            private set
            {
                if (_currentVerseRef == value) return;
                _currentVerseRef = value;
                OnPropertyChanged();
            }
        }

        public string CurrentVerseText
        {
            get => _currentVerseText;
            private set
            {
                if (_currentVerseText == value) return;
                _currentVerseText = value;
                OnPropertyChanged();
            }
        }

        public string UserTypedText
        {
            get => _userTypedText;
            set
            {
                if (_userTypedText == value) return;
                _userTypedText = value;
                OnPropertyChanged();
            }
        }

        public bool IsTypingLocked
        {
            get => _isTypingLocked;
            private set
            {
                if (_isTypingLocked == value) return;
                _isTypingLocked = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsSetCompleted
        {
            get => _isSetCompleted;
            private set
            {
                if (_isSetCompleted == value) return;
                _isSetCompleted = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsCompletionPopupVisible
        {
            get => _isCompletionPopupVisible;
            private set
            {
                if (_isCompletionPopupVisible == value) return;
                _isCompletionPopupVisible = value;
                OnPropertyChanged();
            }
        }

        public string CompletionPopupText
        {
            get => _completionPopupText;
            private set
            {
                if (_completionPopupText == value) return;
                _completionPopupText = value;
                OnPropertyChanged();
            }
        }

        public int CorrectCount
        {
            get => _correctCount;
            private set
            {
                if (_correctCount == value) return;
                _correctCount = value;
                OnPropertyChanged();
            }
        }

        public int WrongCount
        {
            get => _wrongCount;
            private set
            {
                if (_wrongCount == value) return;
                _wrongCount = value;
                OnPropertyChanged();
            }
        }

        public int TotalCount
        {
            get => _totalCount;
            private set
            {
                if (_totalCount == value) return;
                _totalCount = value;
                OnPropertyChanged();
            }
        }

        public bool IsStatsVisible
        {
            get => _isStatsVisible;
            private set
            {
                if (_isStatsVisible == value) return;
                _isStatsVisible = value;
                OnPropertyChanged();
            }
        }

        public string StatsSummary
        {
            get => _statsSummary;
            private set
            {
                if (_statsSummary == value) return;
                _statsSummary = value;
                OnPropertyChanged();
            }
        }

        public CourseSelectViewModel(Action<object> navigate)
        {
            _navigate = navigate;

            for (int i = 1; i <= VerseCatalog.MAX_COURSE; i++)
                Courses.Add($"{i}과정");

            Days.Add(ALL_DAY_TEXT);
            for (int d = 1; d <= VerseCatalog.MAX_DAY; d++)
                Days.Add($"{d}일차");

            SelectedCourse = null;
            SelectedDay = null;

            StartLearningCommand = new RelayCommand(StartLearning, _ => true);
            NextVerseCommand = new RelayCommand(_ => NextVerse(), _ => CanNextVerse());
        }

        private void StartLearning(object? _)
        {
            if (!ValidateSelection(clearOnly: false)) return;

            int course = ParseCourseNo(SelectedCourse!);
            _verses = BuildVerseList(course, SelectedDay!);

            _currentVerse = null;
            IsSetCompleted = false;
            IsCompletionPopupVisible = false;
            IsStatsVisible = false;
            StatsSummary = string.Empty;

            Results.Clear();
            CorrectCount = 0;
            WrongCount = 0;
            TotalCount = _verses.Count;

            if (_verses.Count == 0)
            {
                CurrentVerseRef = string.Empty;
                CurrentVerseText = NO_VERSE_TEXT;
                IsTypingLocked = true;
                UserTypedText = string.Empty;
                _queue.Clear();
                CommandManager.InvalidateRequerySuggested();
                return;
            }

            ResetQueueOnce();
            ShowNextVerseInternal();

            IsTypingLocked = false;
            CommandManager.InvalidateRequerySuggested();
        }

        private void NextVerse()
        {
            if (!CanNextVerse()) return;

            EvaluateCurrentVerse();

            if (_queue.Count > 0)
            {
                ShowNextVerseInternal();
                return;
            }

            CompleteSet();
        }

        private bool CanNextVerse() => !IsTypingLocked && !IsSetCompleted && _currentVerse != null;

        private void ShowNextVerseInternal()
        {
            if (_queue.Count == 0) return;

            Verse verse = _queue.Dequeue();
            _currentVerse = verse;

            CurrentVerseRef = verse.Ref;
            CurrentVerseText = verse.Text;

            UserTypedText = string.Empty;
            CommandManager.InvalidateRequerySuggested();
        }

        private void EvaluateCurrentVerse()
        {
            if (_currentVerse == null) return;

            string expected = Normalize(_currentVerse.Text);
            string typed = Normalize(UserTypedText);

            bool isCorrect = string.Equals(expected, typed, StringComparison.Ordinal);

            Results.Add(new CourseTypingResultItem
            {
                Ref = _currentVerse.Ref,
                Expected = _currentVerse.Text,
                Typed = UserTypedText,
                IsCorrect = isCorrect
            });

            if (isCorrect) CorrectCount++;
            else WrongCount++;
        }

        private void CompleteSet()
        {
            IsSetCompleted = true;
            IsTypingLocked = true;

            IsStatsVisible = true;
            StatsSummary = $"총 {TotalCount}개 중 정답 {CorrectCount}개 / 오답 {WrongCount}개";

            CommandManager.InvalidateRequerySuggested();
            _ = ShowCompletionPopupAsync();
        }

        private async Task ShowCompletionPopupAsync()
        {
            IsCompletionPopupVisible = true;
            await Task.Delay(COMPLETION_POPUP_MILLISECONDS);
            IsCompletionPopupVisible = false;
        }

        private void ResetQueueOnce()
        {
            List<Verse> list = _verses.ToList();
            Shuffle(list);
            _queue = new Queue<Verse>(list);
        }

        private void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private List<Verse> BuildVerseList(int course, string selectedDay)
        {
            if (selectedDay == ALL_DAY_TEXT)
            {
                List<Verse> all = new List<Verse>();
                for (int day = 1; day <= VerseCatalog.MAX_DAY; day++)
                {
                    IReadOnlyList<Verse> list = UseAccumulated
                        ? VerseCatalog.GetAccumulated(course, day)
                        : VerseCatalog.GetAdds(course, day);

                    all.AddRange(list);
                }

                return all.GroupBy(v => v.Ref).Select(g => g.First()).ToList();
            }

            int oneDay = ParseDayNo(selectedDay);

            IReadOnlyList<Verse> dayList = UseAccumulated
                ? VerseCatalog.GetAccumulated(course, oneDay)
                : VerseCatalog.GetAdds(course, oneDay);

            return dayList.ToList();
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

        private static string Normalize(string? s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;

            return s.Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Trim();
        }

        private bool ValidateSelection(bool clearOnly)
        {
            bool ok = !(string.IsNullOrWhiteSpace(SelectedCourse) || string.IsNullOrWhiteSpace(SelectedDay));

            if (ok)
            {
                CourseErrorMessage = null;
                return true;
            }

            if (clearOnly) return false;

            if (string.IsNullOrWhiteSpace(SelectedCourse) && string.IsNullOrWhiteSpace(SelectedDay))
                CourseErrorMessage = EMPTY_SELECTION_TEXT;
            else if (string.IsNullOrWhiteSpace(SelectedCourse))
                CourseErrorMessage = EMPTY_COURSE_TEXT;
            else
                CourseErrorMessage = EMPTY_DAY_TEXT;

            return false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
