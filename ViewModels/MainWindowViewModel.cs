using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using ScriptureTyping.Services;
using ScriptureTyping.ViewModels.Games;
using ScriptureTyping.ViewModels.RecitingMusic;

namespace ScriptureTyping.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private object? _currentContentViewModel;
        private readonly string _planImagePath;
        private readonly ScheduleService _scheduleService;
        private readonly DispatcherTimer _scheduleTimer;

        private string _statusMessage = string.Empty;
        private string _footerRight = string.Empty;
        private string _currentTimeText = string.Empty;
        private string _todayScheduleText = string.Empty;
        private string _currentScheduleText = string.Empty;
        private string _nextScheduleText = string.Empty;
        private string _currentYearText = string.Empty;
        private string _currentMonthText = string.Empty;
        private string _currentDayText = string.Empty;
        private string _currentClockText = string.Empty;

        public string Title { get; } = "42기 사무엘학교";

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (Equals(_statusMessage, value))
                {
                    return;
                }

                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public string FooterRight
        {
            get => _footerRight;
            set
            {
                if (Equals(_footerRight, value))
                {
                    return;
                }

                _footerRight = value;
                OnPropertyChanged();
            }
        }

        public string CurrentTimeText
        {
            get => _currentTimeText;
            set
            {
                if (Equals(_currentTimeText, value))
                {
                    return;
                }

                _currentTimeText = value;
                OnPropertyChanged();
            }
        }

        public string TodayScheduleText
        {
            get => _todayScheduleText;
            set
            {
                if (Equals(_todayScheduleText, value))
                {
                    return;
                }

                _todayScheduleText = value;
                OnPropertyChanged();
            }
        }

        public string CurrentScheduleText
        {
            get => _currentScheduleText;
            set
            {
                if (Equals(_currentScheduleText, value))
                {
                    return;
                }

                _currentScheduleText = value;
                OnPropertyChanged();
            }
        }

        public string NextScheduleText
        {
            get => _nextScheduleText;
            set
            {
                if (Equals(_nextScheduleText, value))
                {
                    return;
                }

                _nextScheduleText = value;
                OnPropertyChanged();
            }
        }

        public string CurrentYearText
        {
            get => _currentYearText;
            set
            {
                if (Equals(_currentYearText, value))
                {
                    return;
                }

                _currentYearText = value;
                OnPropertyChanged();
            }
        }

        public string CurrentMonthText
        {
            get => _currentMonthText;
            set
            {
                if (Equals(_currentMonthText, value))
                {
                    return;
                }

                _currentMonthText = value;
                OnPropertyChanged();
            }
        }

        public string CurrentDayText
        {
            get => _currentDayText;
            set
            {
                if (Equals(_currentDayText, value))
                {
                    return;
                }

                _currentDayText = value;
                OnPropertyChanged();
            }
        }

        public string CurrentClockText
        {
            get => _currentClockText;
            set
            {
                if (Equals(_currentClockText, value))
                {
                    return;
                }

                _currentClockText = value;
                OnPropertyChanged();
            }
        }

        public MainMenuViewModel MenuViewModel { get; }

        public ICommand GoHomeCommand { get; }

        public bool IsHomeVisible => CurrentContentViewModel is null;

        public string PlanImagePath => _planImagePath;

        public string PlanImageMessage
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_planImagePath))
                {
                    return string.Empty;
                }

                return "plan.jpg 파일을 찾지 못했습니다.\nAssets\\Plan\\plan.jpg 위치를 확인해 주세요.";
            }
        }

        public object? CurrentContentViewModel
        {
            get => _currentContentViewModel;
            private set
            {
                if (ReferenceEquals(_currentContentViewModel, value))
                {
                    return;
                }

                _currentContentViewModel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsHomeVisible));
            }
        }

        public MainWindowViewModel()
        {
            MenuViewModel = new MainMenuViewModel(this, ExitApp);
            GoHomeCommand = new ActionCommand(NavigateToHome);

            _planImagePath = ResolvePlanImagePath();
            _scheduleService = new ScheduleService();

            CurrentContentViewModel = null;

            StatusMessage = "가능";
            FooterRight = "v0.1";

            UpdateScheduleInfo();

            _scheduleTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _scheduleTimer.Tick += OnScheduleTimerTick;
            _scheduleTimer.Start();
        }

        public void NavigateToHome()
        {
            StopCurrentContentIfNeeded();
            CurrentContentViewModel = null;
        }

        public void NavigateToContent(object vm)
        {
            StopCurrentContentIfNeeded();
            CurrentContentViewModel = vm;
        }

        public void NavigateTo(object vm)
        {
            NavigateToContent(vm);
        }

        public void NavigateToContentViewModel(object vm)
        {
            NavigateToContent(vm);
        }

        public void NavigateToGamesHub()
        {
            NavigateToContent(new GamesHubViewModel(this));
        }

        public void NavigateToCourseSelect()
        {
            NavigateToContent(new CourseSelectViewModel(o => NavigateToContent(o!)));
        }

        private void OnScheduleTimerTick(object? sender, EventArgs e)
        {
            UpdateScheduleInfo();
        }

        private void UpdateScheduleInfo()
        {
            DateTime now = DateTime.Now;
            var todaySchedules = _scheduleService.GetTodaySchedules(now);
            var currentSchedule = _scheduleService.GetCurrentSchedule(now);
            var nextSchedule = _scheduleService.GetNextSchedule(now);

            CurrentYearText = now.ToString("yyyy");
            CurrentMonthText = now.ToString("MM");
            CurrentDayText = now.ToString("dd");
            CurrentClockText = _scheduleService.FormatTime(now.TimeOfDay);
            CurrentTimeText = $"현재 시간: {CurrentClockText}";

            if (todaySchedules.Count > 0)
            {
                var first = todaySchedules[0];
                TodayScheduleText = $"오늘 일정: {first.DayLabel} / {first.DateLabel}";
            }
            else
            {
                TodayScheduleText = $"오늘 일정: {now:MM/dd} / 등록된 일정 없음";
            }

            if (currentSchedule is null)
            {
                CurrentScheduleText = "현재 일정: 현재 진행 중인 일정 없음";
            }
            else
            {
                CurrentScheduleText = $"현재 일정: {_scheduleService.FormatRange(currentSchedule)}  {currentSchedule.Title}";
            }

            if (nextSchedule is null)
            {
                NextScheduleText = "다음 일정: 오늘 남은 일정 없음";
            }
            else
            {
                NextScheduleText = $"다음 일정: {_scheduleService.FormatRange(nextSchedule)}  {nextSchedule.Title}";
            }
        }

        private void StopCurrentContentIfNeeded()
        {
            if (CurrentContentViewModel is RecitingMusicViewModel recitingMusicViewModel)
            {
                recitingMusicViewModel.StopPlaybackOnLeaveView();
            }
        }

        private static string ResolvePlanImagePath()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string[] candidates =
            {
                Path.Combine(baseDirectory, "Assets", "Plan", "plan.jpg"),
                Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\..\Assets\Plan\plan.jpg")),
                Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\..\..\Assets\Plan\plan.jpg")),
                Path.Combine(Environment.CurrentDirectory, "Assets", "Plan", "plan.jpg")
            };

            foreach (string candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return string.Empty;
        }

        private void ExitApp()
        {
            System.Windows.Application.Current.Shutdown();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed class ActionCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public ActionCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute();
        }
    }
}