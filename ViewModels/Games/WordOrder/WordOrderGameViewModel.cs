using ScriptureTyping.Commands;
using ScriptureTyping.Data;
using ScriptureTyping.Services;
using ScriptureTyping.ViewModels.Games.WordOrder.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ScriptureTyping.ViewModels.Games.WordOrder
{
    /// <summary>
    /// 목적:
    /// 순서 맞추기 게임의 전체 진행 상태를 관리한다.
    ///
    /// 주요 역할:
    /// - 과정/일차/난이도 선택 UI 관리
    /// - 선택된 말씀 목록 로드
    /// - 문제 목록 생성
    /// - 현재 문제 로드
    /// - 보기 조각 선택 / 제거
    /// - 제출 / 채점
    /// - 힌트
    /// - 다음 문제 이동
    /// - 타이머 관리
    /// </summary>
    public sealed partial class WordOrderGameViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private const string DEFAULT_TITLE = "순서 맞추기";
        private const string ALL_DAY_TEXT = "전일차";
        private const int ALL_DAY_INDEX = 0;

        private readonly MainWindowViewModel? _host;
        private readonly SelectionContext _ctx;
        private readonly WordOrderQuestionFactory _questionFactory;
        private readonly WordOrderTimerController _timerController;

        private readonly List<Verse> _sourceVerses = new();
        private readonly List<WordOrderQuestion> _questions = new();

        private WordOrderQuestion? _currentQuestion;

        private string _title = DEFAULT_TITLE;
        private string _referenceText = string.Empty;
        private string _feedbackText = "과정/일차 말씀을 불러온 뒤 게임을 시작하세요.";
        private string _selectedDifficulty = WordOrderDifficulty.Easy;
        private string _questionProgressText = string.Empty;
        private string _timerText = string.Empty;
        private string _slotGuideText = string.Empty;

        private string? _selectedCourse;
        private string? _selectedDay;
        private string _selectionError = string.Empty;

        private int _currentQuestionIndex;
        private int _remainingHints;
        private int _remainingSubmitCount;
        private int _remainingSeconds;

        private bool _isAnswered;
        private bool _isCorrect;
        private bool _useTimer;
        private bool _isGameStarted;
        private bool _isGameFinished;

        public WordOrderGameViewModel()
            : this(host: null, selectionContext: App.SelectionContext)
        {
        }

        public WordOrderGameViewModel(MainWindowViewModel host)
            : this(host, App.SelectionContext)
        {
        }

        public WordOrderGameViewModel(SelectionContext selectionContext)
            : this(host: null, selectionContext)
        {
        }

        public WordOrderGameViewModel(MainWindowViewModel? host, SelectionContext selectionContext)
        {
            _host = host;
            _ctx = selectionContext ?? throw new ArgumentNullException(nameof(selectionContext));
            _questionFactory = new WordOrderQuestionFactory();
            _timerController = new WordOrderTimerController();

            _timerController.Tick += OnTimerTick;
            _timerController.TimeExpired += OnTimeExpired;

            DifficultyOptions = new ObservableCollection<string>(WordOrderDifficulty.All);

            Courses = new ObservableCollection<string>();
            Days = new ObservableCollection<string>();

            AvailablePieces = new ObservableCollection<WordOrderPieceItem>();
            AnswerPieces = new ObservableCollection<WordOrderPieceItem>();

            BackCommand = new RelayCommand(_ => Back(), _ => true);
            ApplySelectionCommand = new RelayCommand(_ => ApplySelection(), _ => CanApplySelection());

            SelectPieceCommand = new RelayCommand(OnSelectPiece, _ => CanSelectPiece);
            RemovePieceCommand = new RelayCommand(OnRemovePiece, _ => CanRemovePiece);
            ClearAnswerCommand = new RelayCommand(_ => ClearAnswer(), _ => CanClearAnswer());
            SubmitAnswerCommand = new RelayCommand(_ => SubmitAnswer(), _ => CanSubmitAnswer());
            HintCommand = new RelayCommand(_ => UseHint(), _ => CanUseHint());
            NextQuestionCommand = new RelayCommand(_ => MoveNextQuestion(), _ => CanMoveNextQuestion());
            StartGameCommand = new RelayCommand(_ => StartGame(), _ => CanStartGame());

            InitSelectionUi();
            ApplySelectionFromContextOrDefault();
            UpdateStatusTexts();
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        public event Action? BackRequested;

        private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<string> Courses { get; }
        public ObservableCollection<string> Days { get; }
        public ObservableCollection<string> DifficultyOptions { get; }

        /// <summary>
        /// 현재 화면에 남아 있는 보기 조각 목록
        /// </summary>
        public ObservableCollection<WordOrderPieceItem> AvailablePieces { get; }

        /// <summary>
        /// 사용자가 순서대로 쌓은 답안 조각 목록
        /// </summary>
        public ObservableCollection<WordOrderPieceItem> AnswerPieces { get; }

        /// <summary>
        /// XAML 호환용 별칭
        /// </summary>
        public ObservableCollection<WordOrderPieceItem> SelectedPieces => AnswerPieces;

        public ICommand BackCommand { get; }
        public ICommand ApplySelectionCommand { get; }

        public ICommand SelectPieceCommand { get; }
        public ICommand RemovePieceCommand { get; }
        public ICommand ClearAnswerCommand { get; }
        public ICommand SubmitAnswerCommand { get; }
        public ICommand HintCommand { get; }
        public ICommand NextQuestionCommand { get; }
        public ICommand StartGameCommand { get; }

        /// <summary>
        /// XAML 호환용 별칭
        /// </summary>
        public ICommand ClearAllCommand => ClearAnswerCommand;

        /// <summary>
        /// XAML 호환용 별칭
        /// </summary>
        public ICommand SubmitCommand => SubmitAnswerCommand;

        public string? SelectedCourse
        {
            get => _selectedCourse;
            set
            {
                if (_selectedCourse == value)
                {
                    return;
                }

                _selectedCourse = value;
                SelectionError = string.Empty;
                NotifyPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string? SelectedDay
        {
            get => _selectedDay;
            set
            {
                if (_selectedDay == value)
                {
                    return;
                }

                _selectedDay = value;
                SelectionError = string.Empty;
                NotifyPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
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

        /// <summary>
        /// 제목
        /// </summary>
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

        /// <summary>
        /// 현재 문제 장절
        /// </summary>
        public string ReferenceText
        {
            get => _referenceText;
            private set
            {
                if (_referenceText == value)
                {
                    return;
                }

                _referenceText = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(QuestionText));
            }
        }

        /// <summary>
        /// 상태/피드백 문구
        /// </summary>
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
                NotifyPropertyChanged(nameof(GuideText));
            }
        }

        /// <summary>
        /// 현재 난이도
        /// 적용 버튼을 누르기 전까지는 UI 선택값만 바뀌고 게임은 즉시 다시 시작하지 않는다.
        /// 실제 반영은 ApplySelection()에서 StartGame()을 호출할 때 일어난다.
        /// </summary>
        public string SelectedDifficulty
        {
            get => _selectedDifficulty;
            set
            {
                string nextValue = string.IsNullOrWhiteSpace(value)
                    ? WordOrderDifficulty.Easy
                    : value;

                if (string.Equals(_selectedDifficulty, nextValue, StringComparison.Ordinal))
                {
                    return;
                }

                _selectedDifficulty = nextValue;
                SelectionError = string.Empty;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(DifficultyText));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// 진행도 표시
        /// </summary>
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

        /// <summary>
        /// 타이머 표시 텍스트
        /// </summary>
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

        /// <summary>
        /// 슬롯 안내 문구
        /// </summary>
        public string SlotGuideText
        {
            get => _slotGuideText;
            private set
            {
                if (_slotGuideText == value)
                {
                    return;
                }

                _slotGuideText = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 남은 힌트 수
        /// </summary>
        public int RemainingHints
        {
            get => _remainingHints;
            private set
            {
                if (_remainingHints == value)
                {
                    return;
                }

                _remainingHints = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(HintSummaryText));
                NotifyPropertyChanged(nameof(HintText));
            }
        }

        /// <summary>
        /// 남은 제출 기회
        /// </summary>
        public int RemainingSubmitCount
        {
            get => _remainingSubmitCount;
            private set
            {
                if (_remainingSubmitCount == value)
                {
                    return;
                }

                _remainingSubmitCount = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(SubmitSummaryText));
                NotifyPropertyChanged(nameof(TryText));
            }
        }

        /// <summary>
        /// 남은 시간(초)
        /// </summary>
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
            }
        }

        /// <summary>
        /// 현재 문제가 제출되었는지
        /// </summary>
        public bool IsAnswered
        {
            get => _isAnswered;
            private set
            {
                if (_isAnswered == value)
                {
                    return;
                }

                _isAnswered = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(CanSelectPiece));
                NotifyPropertyChanged(nameof(CanRemovePiece));
                NotifyPropertyChanged(nameof(CanClearAll));
                NotifyPropertyChanged(nameof(CanSubmit));
                NotifyPropertyChanged(nameof(CanGoNext));
                NotifyPropertyChanged(nameof(IsHintEnabled));
            }
        }

        /// <summary>
        /// 현재 문제 정답 여부
        /// </summary>
        public bool IsCorrect
        {
            get => _isCorrect;
            private set
            {
                if (_isCorrect == value)
                {
                    return;
                }

                _isCorrect = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 타이머 사용 여부
        /// </summary>
        public bool UseTimer
        {
            get => _useTimer;
            private set
            {
                if (_useTimer == value)
                {
                    return;
                }

                _useTimer = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsTimerVisible));
            }
        }

        /// <summary>
        /// 타이머 영역 표시 여부
        /// </summary>
        public bool IsTimerVisible => UseTimer;

        /// <summary>
        /// 게임 시작 여부
        /// </summary>
        public bool IsGameStarted
        {
            get => _isGameStarted;
            private set
            {
                if (_isGameStarted == value)
                {
                    return;
                }

                _isGameStarted = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 게임 종료 여부
        /// </summary>
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
                NotifyPropertyChanged(nameof(CanGoNext));
            }
        }

        /// <summary>
        /// 상단 상태용 텍스트
        /// </summary>
        public string HintSummaryText => $"힌트: {RemainingHints}회";
        public string SubmitSummaryText => $"제출 기회: {RemainingSubmitCount}회";

        /// <summary>
        /// 기존 XAML 호환용 별칭
        /// </summary>
        public string DifficultyText => SelectedDifficulty;
        public string HintText => HintSummaryText;
        public string TryText => SubmitSummaryText;
        public string GuideText => string.IsNullOrWhiteSpace(SlotGuideText) ? FeedbackText : SlotGuideText;
        public string QuestionText => ReferenceText;

        public bool CanSelectPiece =>
            _currentQuestion is not null &&
            !IsAnswered;

        public bool CanRemovePiece =>
            _currentQuestion is not null &&
            !IsAnswered;

        public bool IsHintEnabled => CanUseHint();
        public bool CanClearAll => CanClearAnswer();
        public bool CanSubmit => CanSubmitAnswer();
        public bool CanGoNext => CanMoveNextQuestion();

        private void InitSelectionUi()
        {
            Courses.Clear();
            for (int i = 1; i <= VerseCatalog.MAX_COURSE; i++)
            {
                Courses.Add($"{i}과정");
            }

            Days.Clear();
            for (int d = 1; d <= VerseCatalog.MAX_DAY; d++)
            {
                Days.Add($"{d}일차");
            }

            Days.Add(ALL_DAY_TEXT);
        }

        private void ApplySelectionFromContextOrDefault()
        {
            SelectedDifficulty = WordOrderDifficulty.Easy;

            if (_ctx.HasSelection)
            {
                SelectedCourse = _ctx.SelectedCourseId;
                int dayIndex = _ctx.SelectedDayIndex ?? 0;
                SelectedDay = dayIndex == 0 ? ALL_DAY_TEXT : $"{dayIndex}일차";
                return;
            }

            SelectedCourse = Courses.FirstOrDefault();
            SelectedDay = Days.FirstOrDefault();
        }

        private bool CanApplySelection()
        {
            return !string.IsNullOrWhiteSpace(SelectedCourse)
                && !string.IsNullOrWhiteSpace(SelectedDay)
                && !string.IsNullOrWhiteSpace(SelectedDifficulty);
        }

        private void ApplySelection()
        {
            if (!CanApplySelection())
            {
                SelectionError = "과정/일차/단계를 선택해 주세요.";
                return;
            }

            int courseNo = ParseCourseNo(SelectedCourse!);
            int dayIndex = SelectedDay == ALL_DAY_TEXT ? ALL_DAY_INDEX : ParseDayNo(SelectedDay!);

            IReadOnlyList<Verse> verses = BuildVerseList(courseNo, SelectedDay!);

            if (verses.Count == 0)
            {
                SelectionError = "해당 선택에 구절이 없습니다.";

                StopTimer();
                AvailablePieces.Clear();
                AnswerPieces.Clear();
                _questions.Clear();
                _currentQuestion = null;

                IsGameStarted = false;
                IsGameFinished = false;
                IsAnswered = false;
                IsCorrect = false;

                ReferenceText = string.Empty;
                FeedbackText = "구절이 없습니다.";
                QuestionProgressText = string.Empty;
                SlotGuideText = string.Empty;
                TimerText = string.Empty;
                RemainingHints = 0;
                RemainingSubmitCount = 0;
                RemainingSeconds = 0;
                UseTimer = false;

                RaiseSelectionAndGameUi();
                return;
            }

            List<VerseItem> items = verses
                .Select(v => new VerseItem { Ref = v.Ref, Text = v.Text })
                .ToList();

            _ctx.SetSelection(SelectedCourse!, dayIndex, items);

            SetVerseSource(verses);
            StartGame();
        }

        /// <summary>
        /// 목적:
        /// 게임에 사용할 Verse 목록을 외부에서 주입한다.
        /// </summary>
        public void SetVerseSource(IEnumerable<Verse> verses)
        {
            _sourceVerses.Clear();

            if (verses is not null)
            {
                _sourceVerses.AddRange(verses.Where(x => x is not null));
            }

            FeedbackText = _sourceVerses.Count == 0
                ? "선택된 말씀 데이터가 없습니다."
                : $"총 {_sourceVerses.Count}개 말씀을 불러왔습니다.";

            UpdateCommandStates();
        }

        /// <summary>
        /// 목적:
        /// 현재 난이도 기준으로 문제 목록을 만들고 첫 문제를 시작한다.
        /// </summary>
        public void StartGame()
        {
            StopTimer();

            AvailablePieces.Clear();
            AnswerPieces.Clear();
            _questions.Clear();
            _currentQuestion = null;
            _currentQuestionIndex = 0;

            IsAnswered = false;
            IsCorrect = false;
            IsGameFinished = false;

            if (_sourceVerses.Count == 0)
            {
                IsGameStarted = false;
                FeedbackText = "게임에 사용할 말씀이 없습니다.";
                ReferenceText = string.Empty;
                QuestionProgressText = string.Empty;
                SlotGuideText = string.Empty;
                TimerText = string.Empty;
                UpdateCommandStates();
                return;
            }

            BuildQuestions();

            if (_questions.Count == 0)
            {
                IsGameStarted = false;
                FeedbackText = "문제를 만들 수 없습니다.";
                ReferenceText = string.Empty;
                QuestionProgressText = string.Empty;
                SlotGuideText = string.Empty;
                TimerText = string.Empty;
                UpdateCommandStates();
                return;
            }

            IsGameStarted = true;
            LoadQuestion(0);
        }

        /// <summary>
        /// 현재 문제를 로드한다.
        /// </summary>
        private void LoadQuestion(int index)
        {
            StopTimer();

            if (index < 0 || index >= _questions.Count)
            {
                return;
            }

            _currentQuestionIndex = index;
            _currentQuestion = _questions[index];

            AvailablePieces.Clear();
            AnswerPieces.Clear();

            foreach (WordOrderPieceItem piece in _currentQuestion.Pieces)
            {
                piece.ResetPlacement();
                AvailablePieces.Add(piece);
            }

            IsAnswered = false;
            IsCorrect = false;
            RemainingHints = _currentQuestion.HintCount;
            RemainingSubmitCount = _questionFactory.GetRules(_currentQuestion.Difficulty).MaxSubmitCount;
            UseTimer = _currentQuestion.UseTimer;
            RemainingSeconds = _currentQuestion.TimeLimitSeconds;

            _timerController.Configure(RemainingSeconds);

            Title = DEFAULT_TITLE;
            ReferenceText = _currentQuestion.ReferenceText;
            FeedbackText = GetInitialGuideText(_currentQuestion.Difficulty);
            ApplyFixedPiecesIfNeeded(_currentQuestion);

            UpdateStatusTexts();

            if (UseTimer && RemainingSeconds > 0)
            {
                UpdateTimerText();
                _timerController.Start();
            }
            else
            {
                TimerText = string.Empty;
            }

            UpdateCommandStates();
        }

        private void BuildQuestions()
        {
            _questions.Clear();

            foreach (Verse verse in _sourceVerses)
            {
                WordOrderQuestion question = _questionFactory.CreateQuestion(
                    verse,
                    SelectedDifficulty,
                    _sourceVerses);

                _questions.Add(question);
            }
        }

        private void ApplyFixedPiecesIfNeeded(WordOrderQuestion question)
        {
            if (!question.IsFirstPieceFixed || question.CorrectSequence.Count == 0)
            {
                return;
            }

            string firstText = question.CorrectSequence[0];
            WordOrderPieceItem? firstPiece = AvailablePieces
                .FirstOrDefault(x => !x.IsDistractor && string.Equals(x.Text, firstText, StringComparison.Ordinal));

            if (firstPiece is null)
            {
                return;
            }

            AvailablePieces.Remove(firstPiece);
            firstPiece.PlaceAt(0);
            AnswerPieces.Add(firstPiece);

            RefreshPlacedIndexes();

            FeedbackText = "첫 조각이 고정되었습니다. 나머지 순서를 맞춰보세요.";
        }

        private void OnSelectPiece(object? parameter)
        {
            if (IsAnswered || _currentQuestion is null)
            {
                return;
            }

            if (parameter is not WordOrderPieceItem piece)
            {
                return;
            }

            if (!AvailablePieces.Contains(piece))
            {
                return;
            }

            AvailablePieces.Remove(piece);
            AnswerPieces.Add(piece);

            RefreshPlacedIndexes();
            UpdateStatusTexts();
            UpdateCommandStates();
        }

        private void OnRemovePiece(object? parameter)
        {
            if (IsAnswered || _currentQuestion is null)
            {
                return;
            }

            if (parameter is not WordOrderPieceItem piece)
            {
                return;
            }

            if (!AnswerPieces.Contains(piece))
            {
                return;
            }

            if (_currentQuestion.IsFirstPieceFixed && piece.CurrentPlacedIndex == 0)
            {
                FeedbackText = "고정된 첫 조각은 제거할 수 없습니다.";
                return;
            }

            AnswerPieces.Remove(piece);
            piece.ResetPlacement();
            AvailablePieces.Add(piece);

            RefreshPlacedIndexes();
            UpdateStatusTexts();
            UpdateCommandStates();
        }

        private void ClearAnswer()
        {
            if (IsAnswered || _currentQuestion is null)
            {
                return;
            }

            List<WordOrderPieceItem> removablePieces = AnswerPieces
                .Where(x => !(_currentQuestion.IsFirstPieceFixed && x.CurrentPlacedIndex == 0))
                .ToList();

            foreach (WordOrderPieceItem piece in removablePieces)
            {
                AnswerPieces.Remove(piece);
                piece.ResetPlacement();
                AvailablePieces.Add(piece);
            }

            RefreshPlacedIndexes();
            FeedbackText = "답안을 초기화했습니다.";
            UpdateStatusTexts();
            UpdateCommandStates();
        }

        private void SubmitAnswer()
        {
            if (_currentQuestion is null || IsAnswered)
            {
                return;
            }

            if (AnswerPieces.Count != _currentQuestion.CorrectSequence.Count)
            {
                FeedbackText = $"정답 칸을 모두 채워야 합니다. 현재 {AnswerPieces.Count}/{_currentQuestion.CorrectSequence.Count}";
                return;
            }

            RemainingSubmitCount = Math.Max(0, RemainingSubmitCount - 1);

            bool containsDistractor = AnswerPieces.Any(x => x.IsDistractor);
            bool isCorrectSequence = IsCurrentAnswerCorrect(_currentQuestion);

            IsAnswered = true;
            IsCorrect = !containsDistractor && isCorrectSequence;

            StopTimer();

            if (IsCorrect)
            {
                FeedbackText = GetCorrectFeedbackText(_currentQuestion.Difficulty);
            }
            else
            {
                FeedbackText = GetWrongFeedbackText(
                    _currentQuestion,
                    AnswerPieces,
                    containsDistractor);
            }

            if (!IsCorrect && RemainingSubmitCount > 0)
            {
                IsAnswered = false;
                FeedbackText += $" 다시 시도하세요. 남은 제출 기회 {RemainingSubmitCount}회";
            }

            if (!IsCorrect && RemainingSubmitCount == 0)
            {
                FeedbackText += $" 정답: {string.Join(" / ", _currentQuestion.CorrectSequence)}";
            }

            UpdateCommandStates();
        }

        private void UseHint()
        {
            if (_currentQuestion is null || IsAnswered || RemainingHints <= 0)
            {
                return;
            }

            int nextIndex = AnswerPieces.Count;

            if (nextIndex < 0 || nextIndex >= _currentQuestion.CorrectSequence.Count)
            {
                return;
            }

            string targetText = _currentQuestion.CorrectSequence[nextIndex];
            WordOrderPieceItem? targetPiece = AvailablePieces
                .FirstOrDefault(x => !x.IsDistractor && string.Equals(x.Text, targetText, StringComparison.Ordinal));

            if (targetPiece is null)
            {
                FeedbackText = "사용할 수 있는 힌트 조각이 없습니다.";
                return;
            }

            AvailablePieces.Remove(targetPiece);
            AnswerPieces.Add(targetPiece);

            RemainingHints = Math.Max(0, RemainingHints - 1);
            FeedbackText = $"힌트를 사용했습니다. {nextIndex + 1}번째 조각이 배치되었습니다.";

            RefreshPlacedIndexes();
            UpdateStatusTexts();
            UpdateCommandStates();
        }

        private void MoveNextQuestion()
        {
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

            AvailablePieces.Clear();
            AnswerPieces.Clear();

            _currentQuestion = null;
            IsGameFinished = true;
            ReferenceText = string.Empty;
            QuestionProgressText = $"{_questions.Count}/{_questions.Count}";
            SlotGuideText = string.Empty;
            TimerText = string.Empty;
            FeedbackText = "모든 문제를 완료했습니다.";

            UpdateCommandStates();
        }

        private bool IsCurrentAnswerCorrect(WordOrderQuestion question)
        {
            if (AnswerPieces.Count != question.CorrectSequence.Count)
            {
                return false;
            }

            for (int i = 0; i < question.CorrectSequence.Count; i++)
            {
                if (!string.Equals(AnswerPieces[i].Text, question.CorrectSequence[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private void RefreshPlacedIndexes()
        {
            for (int i = 0; i < AnswerPieces.Count; i++)
            {
                AnswerPieces[i].PlaceAt(i);
            }

            NotifyPropertyChanged(nameof(SelectedPieces));
        }

        private void OnTimerTick(int remainingSeconds)
        {
            if (!UseTimer)
            {
                return;
            }

            RemainingSeconds = remainingSeconds;
            UpdateTimerText();
        }

        private void OnTimeExpired()
        {
            if (!UseTimer)
            {
                return;
            }

            RemainingSeconds = 0;
            UpdateTimerText();
            StopTimer();

            if (!IsAnswered)
            {
                IsAnswered = true;
                IsCorrect = false;
                RemainingSubmitCount = 0;

                if (_currentQuestion is not null)
                {
                    FeedbackText = $"시간 종료! 정답: {string.Join(" / ", _currentQuestion.CorrectSequence)}";
                }
                else
                {
                    FeedbackText = "시간 종료!";
                }

                UpdateCommandStates();
            }
        }

        private void StopTimer()
        {
            _timerController.Stop();
        }

        private void UpdateStatusTexts()
        {
            if (_currentQuestion is null)
            {
                QuestionProgressText = string.Empty;
                SlotGuideText = string.Empty;
                TimerText = string.Empty;
                NotifyPropertyChanged(nameof(GuideText));
                return;
            }

            QuestionProgressText = $"{_currentQuestionIndex + 1} / {_questions.Count}";
            SlotGuideText = $"정답 칸: {AnswerPieces.Count} / {_currentQuestion.CorrectSequence.Count}";
            NotifyPropertyChanged(nameof(GuideText));
            UpdateTimerText();
        }

        private void UpdateTimerText()
        {
            if (!UseTimer)
            {
                TimerText = string.Empty;
                return;
            }

            TimerText = $"남은 시간: {RemainingSeconds}초";
        }

        private bool CanClearAnswer()
        {
            if (_currentQuestion is null || IsAnswered)
            {
                return false;
            }

            if (_currentQuestion.IsFirstPieceFixed)
            {
                return AnswerPieces.Count > 1;
            }

            return AnswerPieces.Count > 0;
        }

        private bool CanSubmitAnswer()
        {
            return _currentQuestion is not null
                && !IsAnswered
                && RemainingSubmitCount > 0
                && AnswerPieces.Count == _currentQuestion.CorrectSequence.Count;
        }

        private bool CanUseHint()
        {
            return _currentQuestion is not null
                && !IsAnswered
                && RemainingHints > 0
                && AnswerPieces.Count < _currentQuestion.CorrectSequence.Count;
        }

        private bool CanMoveNextQuestion()
        {
            return _questions.Count > 0
                && _currentQuestion is not null
                && (IsAnswered || IsGameFinished);
        }

        private bool CanStartGame()
        {
            return _sourceVerses.Count > 0;
        }

        private void UpdateCommandStates()
        {
            NotifyPropertyChanged(nameof(HintSummaryText));
            NotifyPropertyChanged(nameof(SubmitSummaryText));
            NotifyPropertyChanged(nameof(IsTimerVisible));
            NotifyPropertyChanged(nameof(HintText));
            NotifyPropertyChanged(nameof(TryText));
            NotifyPropertyChanged(nameof(GuideText));
            NotifyPropertyChanged(nameof(CanSelectPiece));
            NotifyPropertyChanged(nameof(CanRemovePiece));
            NotifyPropertyChanged(nameof(IsHintEnabled));
            NotifyPropertyChanged(nameof(CanClearAll));
            NotifyPropertyChanged(nameof(CanSubmit));
            NotifyPropertyChanged(nameof(CanGoNext));

            CommandManager.InvalidateRequerySuggested();
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

        private IReadOnlyList<Verse> BuildVerseList(int course, string selectedDay)
        {
            if (selectedDay == ALL_DAY_TEXT)
            {
                List<Verse> all = new();

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

        private void RaiseSelectionAndGameUi()
        {
            NotifyPropertyChanged(nameof(SelectedCourse));
            NotifyPropertyChanged(nameof(SelectedDay));
            NotifyPropertyChanged(nameof(SelectedDifficulty));
            NotifyPropertyChanged(nameof(DifficultyText));
            NotifyPropertyChanged(nameof(HintText));
            NotifyPropertyChanged(nameof(TryText));
            NotifyPropertyChanged(nameof(GuideText));
            NotifyPropertyChanged(nameof(QuestionText));
            UpdateCommandStates();
        }

        private static string GetInitialGuideText(string difficulty)
        {
            return difficulty switch
            {
                var x when x == WordOrderDifficulty.Easy => "조각을 순서대로 눌러 문장을 완성하세요.",
                var x when x == WordOrderDifficulty.Normal => "어절 순서를 바르게 맞춰보세요.",
                var x when x == WordOrderDifficulty.Hard => "더 세밀한 순서까지 정확히 맞춰보세요.",
                var x when x == WordOrderDifficulty.VeryHard => "방해 조각에 주의하면서 순서를 맞춰보세요.",
                _ => "조각을 순서대로 눌러 문장을 완성하세요."
            };
        }

        private static string GetCorrectFeedbackText(string difficulty)
        {
            return difficulty switch
            {
                var x when x == WordOrderDifficulty.Easy => "정답입니다!",
                var x when x == WordOrderDifficulty.Normal => "정답입니다!",
                var x when x == WordOrderDifficulty.Hard => "정답입니다!",
                var x when x == WordOrderDifficulty.VeryHard => "정답입니다! 방해 조각 없이 정확히 맞췄습니다.",
                _ => "정답입니다!"
            };
        }

        private static string GetWrongFeedbackText(
            WordOrderQuestion question,
            IReadOnlyList<WordOrderPieceItem> answerPieces,
            bool containsDistractor)
        {
            if (containsDistractor)
            {
                return "오답입니다. 방해 조각이 포함되어 있습니다.";
            }

            return "오답입니다. 조각 순서를 다시 확인하세요.";
        }
    }
}