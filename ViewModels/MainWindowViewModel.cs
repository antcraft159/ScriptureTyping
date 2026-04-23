using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ScriptureTyping.Services;
using ScriptureTyping.ViewModels.Games;
using System.Reflection;
using ScriptureTyping.ViewModels.RecitingMusic;

namespace ScriptureTyping.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private object? _currentContentViewModel;
        private readonly string _planImagePath;
        private readonly ScheduleService _scheduleService;
        private readonly DispatcherTimer _scheduleTimer;
        private readonly AppUpdateService _appUpdateService;

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
        private bool _isCheckingUpdate;

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

        public ICommand CheckUpdateCommand { get; }

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

        public MainWindowViewModel(AppUpdateService appUpdateService, ScheduleService scheduleService)
        {
            _appUpdateService = appUpdateService;
            _scheduleService = scheduleService;

            MenuViewModel = new MainMenuViewModel(this, ExitApp);
            GoHomeCommand = new ActionCommand(NavigateToHome);
            CheckUpdateCommand = new ActionCommand(OnCheckUpdateButtonExecuted);

            _planImagePath = ResolvePlanImagePath();

            CurrentContentViewModel = null;

            StatusMessage = "가능";
            FooterRight = GetFooterVersionText();

            UpdateScheduleInfo();

            _scheduleTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _scheduleTimer.Tick += OnScheduleTimerTick;
            _scheduleTimer.Start();
        }

        public Task CheckForUpdatesOnStartupAsync()
        {
            return CheckForUpdatesInternalAsync(isStartupCheck: true);
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

        private void OnCheckUpdateButtonExecuted()
        {
            _ = CheckForUpdatesInternalAsync(isStartupCheck: false);
        }

        private async Task CheckForUpdatesInternalAsync(bool isStartupCheck)
        {
            if (_isCheckingUpdate)
            {
                return;
            }

            _isCheckingUpdate = true;

            try
            {
                StatusMessage = "업데이트 확인 중...";

                AppUpdateCheckResult result = await _appUpdateService.CheckForUpdatesAsync();

                switch (result.State)
                {
                    case AppUpdateCheckState.NotConfigured:
                        StatusMessage = "업데이트 저장소 설정 필요";

                        if (!isStartupCheck)
                        {
                            MessageBox.Show(
                                "Services/AppUpdateService.cs의 UpdateRepositoryUrl 값을 먼저 네 GitHub 저장소 주소로 바꿔야 합니다.",
                                "Check Update",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }

                        return;

                    case AppUpdateCheckState.NotInstalled:
                        StatusMessage = "설치본 아님";

                        if (!isStartupCheck)
                        {
                            MessageBox.Show(
                                "지금 실행 중인 프로그램은 설치된 Velopack 버전이 아닙니다.\n\nbin\\Debug 실행본 말고, Setup.exe로 설치한 프로그램에서만 자동업데이트가 동작합니다.",
                                "Check Update",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }

                        return;

                    case AppUpdateCheckState.NoUpdate:
                        StatusMessage = "최신 버전 사용 중";

                        if (!isStartupCheck)
                        {
                            string currentVersionText = string.IsNullOrWhiteSpace(result.CurrentVersion)
                                ? "알 수 없음"
                                : result.CurrentVersion;

                            MessageBox.Show(
                                $"현재 최신 버전입니다.\n\n현재 버전: {currentVersionText}",
                                "Check Update",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }

                        return;

                    case AppUpdateCheckState.UpdateAvailable:
                        StatusMessage = "새 버전 발견";

                        if (!result.HasUpdate)
                        {
                            return;
                        }

                        string currentVersion = string.IsNullOrWhiteSpace(result.CurrentVersion)
                            ? "알 수 없음"
                            : result.CurrentVersion;

                        string targetVersion = string.IsNullOrWhiteSpace(result.TargetVersion)
                            ? "알 수 없음"
                            : result.TargetVersion;

                        string releaseNotesPreview = BuildReleaseNotesPreview(result.ReleaseNotes);

                        StringBuilder messageBuilder = new StringBuilder();
                        messageBuilder.AppendLine("새 버전이 있습니다.");
                        messageBuilder.AppendLine();
                        messageBuilder.AppendLine($"현재 버전: {currentVersion}");
                        messageBuilder.AppendLine($"최신 버전: {targetVersion}");

                        if (!string.IsNullOrWhiteSpace(releaseNotesPreview))
                        {
                            messageBuilder.AppendLine();
                            messageBuilder.AppendLine("변경사항:");
                            messageBuilder.AppendLine(releaseNotesPreview);
                        }

                        messageBuilder.AppendLine();
                        messageBuilder.Append("지금 업데이트하시겠습니까?");

                        MessageBoxResult confirm = MessageBox.Show(
                            messageBuilder.ToString(),
                            "Check Update",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (confirm != MessageBoxResult.Yes)
                        {
                            StatusMessage = "업데이트 보류";
                            return;
                        }

                        StatusMessage = "업데이트 다운로드 중...";
                        await _appUpdateService.DownloadUpdatesAsync(result.UpdateInfo!);

                        StatusMessage = "업데이트 적용 중...";
                        _appUpdateService.ApplyUpdatesAndRestart(result.UpdateInfo!);
                        return;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "업데이트 확인 실패";

                if (!isStartupCheck)
                {
                    MessageBox.Show(
                        $"업데이트 처리 중 오류가 발생했습니다.\n\n{ex.Message}",
                        "Check Update",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            finally
            {
                _isCheckingUpdate = false;
            }
        }

        private void OnScheduleTimerTick(object? sender, EventArgs e)
        {
            UpdateScheduleInfo();
        }

        private void UpdateScheduleInfo()
        {
            DateTime now = DateTime.Now;

            CurrentYearText = now.ToString("yyyy");
            CurrentMonthText = now.ToString("MM");
            CurrentDayText = now.ToString("dd");
            CurrentClockText = _scheduleService.FormatTime(now.TimeOfDay);
            CurrentTimeText = $"현재 시간: {CurrentClockText}";

            if (_scheduleService.ShouldShowOpeningCountdown(now))
            {
                CurrentScheduleText = _scheduleService.BuildOpeningCountdownMessage(now);
                NextScheduleText = _scheduleService.GetPreparationSecondaryMessage();
                return;
            }

            if (_scheduleService.IsAllSchedulesFinished(now) || _scheduleService.IsBeforeFirstSchedule(now))
            {
                CurrentScheduleText = _scheduleService.GetFinishedPrimaryMessage();
                NextScheduleText = _scheduleService.GetFinishedSecondaryMessage();
                return;
            }

            var currentSchedule = _scheduleService.GetCurrentSchedule(now);

            if (currentSchedule is null)
            {
                CurrentScheduleText = "현재 일정: 현재 진행 중인 일정 없음";
            }
            else
            {
                CurrentScheduleText = $"현재 일정: {_scheduleService.FormatRange(currentSchedule)}\n\n{currentSchedule.Title}";
            }

            NextScheduleText = _scheduleService.BuildNextScheduleMessage(now);
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

        private static string GetFooterVersionText()
        {
            Assembly assembly = typeof(App).Assembly;

            string versionText =
                assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? assembly.GetName().Version?.ToString(3)
                ?? "0.0.0";

            int plusIndex = versionText.IndexOf('+');
            if (plusIndex >= 0)
            {
                versionText = versionText.Substring(0, plusIndex);
            }

            return $"v{versionText}";
        }
        private static string BuildReleaseNotesPreview(string releaseNotes)
        {
            if (string.IsNullOrWhiteSpace(releaseNotes))
            {
                return string.Empty;
            }

            string normalized = releaseNotes.Replace("\r\n", "\n").Trim();

            if (normalized.Length <= 300)
            {
                return normalized;
            }

            return normalized.Substring(0, 300) + "...";
        }

        private void ExitApp()
        {
            Application.Current.Shutdown();
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