using ScriptureTyping.Commands;
using ScriptureTyping.Data;
using ScriptureTyping.Services;
using ScriptureTyping.ViewModels.Games.VerseMatch.Contracts;
using ScriptureTyping.ViewModels.Games.VerseMatch.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ScriptureTyping.ViewModels.Games.VerseMatch
{
    /// <summary>
    /// 목적:
    /// 구절 짝 맞추기 게임의 전체 진행 상태를 관리한다.
    /// </summary>
    public sealed class VerseMatchGameViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private const string DEFAULT_TITLE = "구절 짝 맞추기";
        private const string DEFAULT_COURSE = "1과정";
        private const string DEFAULT_DAY = "1일차";
        private const string ALL_DAY_TEXT = "전일차";

        private readonly MainWindowViewModel? _host;
        private readonly SelectionContext _selectionContext;
        private readonly VerseMatchQuestionFactory _questionFactory;
        private readonly VerseMatchModeFactory _modeFactory;
        private readonly VerseMatchSelectionService _selectionService;
        private readonly VerseMatchTimerController _timerController;

        private readonly List<Verse> _sourceVerses = new();
        private readonly List<VerseMatchQuestion> _questions = new();
        private readonly List<VerseMatchCardItem> _selectedCards = new();

        private VerseMatchQuestion? _currentQuestion;

        private string _title = DEFAULT_TITLE;
        private string _selectedCourse = DEFAULT_COURSE;
        private string _selectedDay = DEFAULT_DAY;
        private string _selectedDifficulty = VerseMatchDifficulty.Normal;
        private string _feedbackText = "카드를 눌러 장절과 본문을 서로 짝지어 맞추세요.";
        private string _completionMessage = string.Empty;
        private string _questionProgressText = string.Empty;
        private string _timerText = string.Empty;
        private string _pairStatusText = string.Empty;
        private string _scoreText = "점수: 0";
        private string _wrongCountText = "오답: 0회";
        private string _selectionError = string.Empty;

        private int _currentQuestionIndex;
        private int _remainingSeconds;
        private int _score;
        private int _wrongCount;

        private bool _isBusy;
        private bool _isGameFinished;

        public new event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public VerseMatchGameViewModel()
            : this(host: null, selectionContext: App.SelectionContext, title: null)
        {
        }

        public VerseMatchGameViewModel(MainWindowViewModel host)
            : this(host, App.SelectionContext, title: null)
        {
        }

        public VerseMatchGameViewModel(MainWindowViewModel host, string? title)
            : this(host, App.SelectionContext, title)
        {
        }

        public VerseMatchGameViewModel(SelectionContext selectionContext)
            : this(host: null, selectionContext, title: null)
        {
        }

        public VerseMatchGameViewModel(SelectionContext selectionContext, string? title)
            : this(host: null, selectionContext, title)
        {
        }

        public VerseMatchGameViewModel(MainWindowViewModel? host, SelectionContext selectionContext, string? title)
        {
            _host = host;
            _selectionContext = selectionContext ?? throw new ArgumentNullException(nameof(selectionContext));
            _questionFactory = new VerseMatchQuestionFactory();
            _modeFactory = new VerseMatchModeFactory();
            _selectionService = new VerseMatchSelectionService();
            _timerController = new VerseMatchTimerController();

            _timerController.SecondElapsed += OnTimerSecondElapsed;
            _timerController.Expired += OnTimerExpired;

            Courses = new ObservableCollection<string>
            {
                "1과정",
                "2과정",
                "3과정",
                "4과정"
            };

            Days = new ObservableCollection<string>();

            for (int day = 1; day <= VerseCatalog.MAX_DAY; day++)
            {
                Days.Add($"{day}일차");
            }

            Days.Add(ALL_DAY_TEXT);

            DifficultyOptions = new ObservableCollection<string>
            {
                VerseMatchDifficulty.Easy,
                VerseMatchDifficulty.Normal,
                VerseMatchDifficulty.Hard,
                VerseMatchDifficulty.VeryHard,
                VerseMatchDifficulty.SamuelRank1
            };

            Cards = new ObservableCollection<VerseMatchCardItem>();

            InitializeSelections();

            BackCommand = new RelayCommand(_ => Back(), _ => true);
            RestartCommand = new RelayCommand(_ => RestartGame(), _ => true);
            NextQuestionCommand = new RelayCommand(_ => MoveNextQuestion(), _ => CanMoveNextQuestion());
            SelectCardCommand = new RelayCommand(OnSelectCard, _ => CanSelectCard);
            ApplySelectionCommand = new RelayCommand(_ => ApplySelection(), _ => true);

            Title = string.IsNullOrWhiteSpace(title) ? DEFAULT_TITLE : title;

            LoadSourceVerses();
            StartGame();
        }

        public ObservableCollection<VerseMatchCardItem> Cards { get; }
        public ObservableCollection<string> Courses { get; }
        public ObservableCollection<string> Days { get; }
        public ObservableCollection<string> DifficultyOptions { get; }

        public ICommand BackCommand { get; }
        public ICommand RestartCommand { get; }
        public ICommand NextQuestionCommand { get; }
        public ICommand SelectCardCommand { get; }
        public ICommand ApplySelectionCommand { get; }

        public string Title
        {
            get => _title;
            private set
            {
                if (_title == value)
                {
                    return;
                }

                _title = value;
                NotifyPropertyChanged();
            }
        }

        public string SelectedCourse
        {
            get => _selectedCourse;
            set
            {
                string nextValue = string.IsNullOrWhiteSpace(value)
                    ? DEFAULT_COURSE
                    : value;

                if (_selectedCourse == nextValue)
                {
                    return;
                }

                _selectedCourse = nextValue;
                ClearSelectionError();
                NotifyPropertyChanged();
            }
        }

        public string SelectedDay
        {
            get => _selectedDay;
            set
            {
                string nextValue = string.IsNullOrWhiteSpace(value)
                    ? DEFAULT_DAY
                    : value;

                if (_selectedDay == nextValue)
                {
                    return;
                }

                _selectedDay = nextValue;
                ClearSelectionError();
                NotifyPropertyChanged();
            }
        }

        public string SelectedDifficulty
        {
            get => _selectedDifficulty;
            set
            {
                string nextValue = string.IsNullOrWhiteSpace(value)
                    ? VerseMatchDifficulty.Normal
                    : value;

                if (_selectedDifficulty == nextValue)
                {
                    return;
                }

                _selectedDifficulty = nextValue;
                ClearSelectionError();
                NotifyPropertyChanged();
            }
        }

        public string SelectionError
        {
            get => _selectionError;
            private set
            {
                if (_selectionError == value)
                {
                    return;
                }

                _selectionError = value;
                NotifyPropertyChanged();
            }
        }

        public string FeedbackText
        {
            get => _feedbackText;
            private set
            {
                if (_feedbackText == value)
                {
                    return;
                }

                _feedbackText = value;
                NotifyPropertyChanged();
            }
        }

        public string CompletionMessage
        {
            get => _completionMessage;
            private set
            {
                if (_completionMessage == value)
                {
                    return;
                }

                _completionMessage = value;
                NotifyPropertyChanged();
            }
        }

        public string QuestionProgressText
        {
            get => _questionProgressText;
            private set
            {
                if (_questionProgressText == value)
                {
                    return;
                }

                _questionProgressText = value;
                NotifyPropertyChanged();
            }
        }

        public string TimerText
        {
            get => _timerText;
            private set
            {
                if (_timerText == value)
                {
                    return;
                }

                _timerText = value;
                NotifyPropertyChanged();
            }
        }

        public string PairStatusText
        {
            get => _pairStatusText;
            private set
            {
                if (_pairStatusText == value)
                {
                    return;
                }

                _pairStatusText = value;
                NotifyPropertyChanged();
            }
        }

        public string ScoreText
        {
            get => _scoreText;
            private set
            {
                if (_scoreText == value)
                {
                    return;
                }

                _scoreText = value;
                NotifyPropertyChanged();
            }
        }

        public string WrongCountText
        {
            get => _wrongCountText;
            private set
            {
                if (_wrongCountText == value)
                {
                    return;
                }

                _wrongCountText = value;
                NotifyPropertyChanged();
            }
        }

        public int RemainingSeconds
        {
            get => _remainingSeconds;
            private set
            {
                if (_remainingSeconds == value)
                {
                    return;
                }

                _remainingSeconds = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(CanSelectCard));
                NotifyPropertyChanged(nameof(CanGoNext));
            }
        }

        public int Score
        {
            get => _score;
            private set
            {
                if (_score == value)
                {
                    return;
                }

                _score = value;
                ScoreText = $"점수: {_score}";
                NotifyPropertyChanged();
            }
        }

        public int WrongCount
        {
            get => _wrongCount;
            private set
            {
                if (_wrongCount == value)
                {
                    return;
                }

                _wrongCount = value;
                WrongCountText = $"오답: {_wrongCount}회";
                NotifyPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy == value)
                {
                    return;
                }

                _isBusy = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(CanSelectCard));
            }
        }

        public bool IsGameFinished
        {
            get => _isGameFinished;
            private set
            {
                if (_isGameFinished == value)
                {
                    return;
                }

                _isGameFinished = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(CanSelectCard));
                NotifyPropertyChanged(nameof(CanGoNext));
            }
        }

        public bool IsTimerVisible => _currentQuestion is not null && _currentQuestion.UseTimer;
        public bool CanSelectCard => !IsBusy && !IsGameFinished && !IsCurrentQuestionTimeExpired;
        public bool CanGoNext => CanMoveNextQuestion();

        private bool IsCurrentQuestionTimeExpired =>
            _currentQuestion is not null &&
            _currentQuestion.UseTimer &&
            RemainingSeconds == 0;

        private void InitializeSelections()
        {
            string courseText = string.IsNullOrWhiteSpace(_selectionContext.SelectedCourseId)
                ? DEFAULT_COURSE
                : _selectionContext.SelectedCourseId!;

            int dayIndex = _selectionContext.SelectedDayIndex ?? 1;

            SelectedCourse = Courses.Contains(courseText)
                ? courseText
                : DEFAULT_COURSE;

            string dayText = dayIndex == 0 ? ALL_DAY_TEXT : $"{dayIndex}일차";
            SelectedDay = Days.Contains(dayText)
                ? dayText
                : DEFAULT_DAY;

            if (!DifficultyOptions.Contains(SelectedDifficulty))
            {
                SelectedDifficulty = VerseMatchDifficulty.Normal;
            }

            ClearSelectionError();
        }

        private bool ValidateSelection()
        {
            if (string.IsNullOrWhiteSpace(SelectedCourse) || !Courses.Contains(SelectedCourse))
            {
                SelectionError = "과정을 선택하세요.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(SelectedDay) || !Days.Contains(SelectedDay))
            {
                SelectionError = "일차를 선택하세요.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(SelectedDifficulty) || !DifficultyOptions.Contains(SelectedDifficulty))
            {
                SelectionError = "난이도를 선택하세요.";
                return false;
            }

            ClearSelectionError();
            return true;
        }

        private void ApplySelection()
        {
            if (!ValidateSelection())
            {
                return;
            }

            LoadSourceVerses();
            StartGame();
        }

        private void LoadSourceVerses()
        {
            _sourceVerses.Clear();

            IReadOnlyList<Verse> verses = _selectionService.GetSourceVerses(
                SelectedCourse,
                SelectedDay);

            _sourceVerses.AddRange(verses);
        }

        public void StartGame()
        {
            StopTimer();

            Cards.Clear();
            _selectedCards.Clear();
            _questions.Clear();
            _currentQuestion = null;
            _currentQuestionIndex = 0;

            Score = 0;
            WrongCount = 0;
            RemainingSeconds = 0;
            IsBusy = false;
            IsGameFinished = false;
            CompletionMessage = string.Empty;

            IVerseMatchMode mode = _modeFactory.Create(SelectedDifficulty);

            IReadOnlyList<VerseMatchQuestion> questions = _questionFactory.CreateQuestions(
                _sourceVerses,
                mode);

            _questions.AddRange(questions);

            if (_questions.Count == 0)
            {
                FeedbackText = "게임에 사용할 구절이 부족합니다.";
                QuestionProgressText = string.Empty;
                PairStatusText = string.Empty;
                TimerText = string.Empty;
                CompletionMessage = string.Empty;
                NotifyPropertyChanged(nameof(IsTimerVisible));
                NotifyPropertyChanged(nameof(CanGoNext));
                return;
            }

            FeedbackText = "카드를 눌러 장절과 본문을 서로 짝지어 맞추세요.";
            LoadQuestion(0);
        }

        private void RestartGame()
        {
            StartGame();
        }

        private void LoadQuestion(int index)
        {
            StopTimer();

            if (index < 0 || index >= _questions.Count)
            {
                return;
            }

            _selectedCards.Clear();
            Cards.Clear();
            CompletionMessage = string.Empty;

            _currentQuestionIndex = index;
            _currentQuestion = _questions[index];

            foreach (VerseMatchCardItem card in _currentQuestion.Cards)
            {
                card.Reset();
                Cards.Add(card);
            }

            RemainingSeconds = _currentQuestion.TimeLimitSeconds;
            UpdateStatusTexts();

            if (_currentQuestion.UseTimer)
            {
                _timerController.Start(RemainingSeconds);
            }
            else
            {
                TimerText = string.Empty;
            }

            NotifyPropertyChanged(nameof(IsTimerVisible));
            NotifyPropertyChanged(nameof(CanGoNext));
            NotifyPropertyChanged(nameof(CanSelectCard));
        }

        private async void OnSelectCard(object? parameter)
        {
            if (IsBusy || IsGameFinished || IsCurrentQuestionTimeExpired || _currentQuestion is null)
            {
                return;
            }

            if (parameter is not VerseMatchCardItem card)
            {
                return;
            }

            if (card.IsMatched || card.IsSelected)
            {
                return;
            }

            card.Select();
            _selectedCards.Add(card);

            if (_selectedCards.Count < 2)
            {
                FeedbackText = "두 장을 선택해 짝이 맞는지 확인하세요.";
                return;
            }

            IsBusy = true;

            VerseMatchCardItem first = _selectedCards[0];
            VerseMatchCardItem second = _selectedCards[1];

            bool isMatch = IsPairMatched(first, second);

            if (isMatch)
            {
                first.MarkMatched();
                second.MarkMatched();

                Score += 10;
                FeedbackText = "정답입니다! 올바른 장절과 본문을 맞췄습니다.";
            }
            else
            {
                WrongCount += 1;

                if (first.IsFakeCard || second.IsFakeCard)
                {
                    FeedbackText = "가짜 카드가 섞여 있습니다. 다시 확인해 보세요.";
                }
                else
                {
                    FeedbackText = "오답입니다. 다시 확인해 보세요.";
                }

                await Task.Delay(800);

                first.Unselect();
                second.Unselect();
            }

            _selectedCards.Clear();
            UpdateStatusTexts();

            if (_currentQuestion.IsCompleted())
            {
                StopTimer();
                FeedbackText = "현재 문제의 모든 짝을 맞췄습니다.";
                IsBusy = false;
                NotifyPropertyChanged(nameof(CanGoNext));
                return;
            }

            IsBusy = false;
        }

        private static bool IsPairMatched(VerseMatchCardItem first, VerseMatchCardItem second)
        {
            if (first is null || second is null)
            {
                return false;
            }

            if (first.CardId == second.CardId)
            {
                return false;
            }

            if (first.IsFakeCard || second.IsFakeCard)
            {
                return false;
            }

            if (first.CardType == second.CardType)
            {
                return false;
            }

            return string.Equals(first.PairKey, second.PairKey, StringComparison.Ordinal);
        }

        private void MoveNextQuestion()
        {
            if (!CanMoveNextQuestion())
            {
                return;
            }

            if (_currentQuestionIndex + 1 >= _questions.Count)
            {
                FinishGame();
                return;
            }

            LoadQuestion(_currentQuestionIndex + 1);
        }

        private void FinishGame()
        {
            StopTimer();

            IsGameFinished = true;
            _selectedCards.Clear();
            Cards.Clear();

            CompletionMessage = $"게임 완료!\n최종 점수 {Score}점 / 오답 {WrongCount}회";
            FeedbackText = $"게임 완료! 최종 점수 {Score}점 / 오답 {WrongCount}회";
            QuestionProgressText = $"{_questions.Count} / {_questions.Count}";
            PairStatusText = "모든 문제 완료";
            TimerText = string.Empty;

            NotifyPropertyChanged(nameof(IsTimerVisible));
            NotifyPropertyChanged(nameof(CanGoNext));
            NotifyPropertyChanged(nameof(CanSelectCard));
        }

        private void OnTimerSecondElapsed(int remainingSeconds)
        {
            RemainingSeconds = remainingSeconds;
            UpdateTimerText();
        }

        private void OnTimerExpired()
        {
            HandleTimeExpired();
        }

        private void HandleTimeExpired()
        {
            if (_currentQuestion is null)
            {
                return;
            }

            foreach (VerseMatchCardItem card in Cards.Where(x => !x.IsMatched))
            {
                card.Unselect();
            }

            _selectedCards.Clear();
            IsBusy = false;
            WrongCount += 1;
            FeedbackText = "시간 종료! 다음 문제로 넘어가세요.";

            NotifyPropertyChanged(nameof(CanGoNext));
            NotifyPropertyChanged(nameof(CanSelectCard));
        }

        private void UpdateStatusTexts()
        {
            if (_currentQuestion is null)
            {
                QuestionProgressText = string.Empty;
                PairStatusText = string.Empty;
                TimerText = string.Empty;
                return;
            }

            QuestionProgressText = $"{_currentQuestionIndex + 1} / {_questions.Count}";
            PairStatusText = $"남은 짝: {_currentQuestion.GetRemainingPairCount()}";
            UpdateTimerText();
        }

        private void UpdateTimerText()
        {
            if (_currentQuestion is null || !_currentQuestion.UseTimer)
            {
                TimerText = string.Empty;
                return;
            }

            TimerText = $"남은 시간: {RemainingSeconds}초";
        }

        private bool CanMoveNextQuestion()
        {
            if (_currentQuestion is null)
            {
                return false;
            }

            return _currentQuestion.IsCompleted()
                || (_currentQuestion.UseTimer && RemainingSeconds == 0);
        }

        private void StopTimer()
        {
            _timerController.Stop();
        }

        private void Back()
        {
            StopTimer();

            if (_host is not null)
            {
                _host.NavigateTo(new GamesHubViewModel(_host));
            }
        }

        private void ClearSelectionError()
        {
            SelectionError = string.Empty;
        }
    }
}