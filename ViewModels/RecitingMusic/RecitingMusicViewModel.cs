using ScriptureTyping.Commands;
using ScriptureTyping.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;

namespace ScriptureTyping.ViewModels.RecitingMusic
{
    /// <summary>
    /// 목적:
    /// 동요 목록, 필터, 선택 곡 상세, 통계 카드, mp3 재생 상태를 관리한다.
    /// </summary>
    public sealed class RecitingMusicViewModel : INotifyPropertyChanged
    {
        private const string STATUS_NO_DATA = "노래 데이터가 없습니다.";
        private const string STATUS_SELECT_SONG = "곡을 선택하세요.";
        private const string STATUS_SELECT_FIRST = "먼저 곡을 선택하세요.";
        private const string STATUS_PLAYABLE = "재생 가능";
        private const string STATUS_UNAVAILABLE = "불가능";

        private readonly ObservableCollection<RecitingMusicItemViewModel> _allItems;
        private readonly AudioPlaybackService _audioPlaybackService;
        private readonly RecitingMusicDataService _recitingMusicDataService;
        private readonly DispatcherTimer _playbackTimer;

        private string? _selectedCourse;
        private string? _selectedDay;
        private string _currentFilterText = string.Empty;
        private RecitingMusicItemViewModel? _selectedItem;
        private string _currentMp3FullPath = string.Empty;
        private string _playbackStatusText = STATUS_SELECT_SONG;
        private bool _isPlaying;

        private string _currentTimeText = "00:00";
        private string _totalDurationText = "00:00";
        private double _playbackProgressMaximum = 1;
        private double _seekBarValue;
        private bool _isSeekDragging;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<string> Courses { get; }

        public ObservableCollection<string> Days { get; }

        public ObservableCollection<RecitingMusicItemViewModel> FilteredItems { get; }

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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        public string CurrentFilterText
        {
            get => _currentFilterText;
            set
            {
                if (_currentFilterText == value)
                {
                    return;
                }

                _currentFilterText = value;
                OnPropertyChanged();
            }
        }

        public RecitingMusicItemViewModel? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem == value)
                {
                    return;
                }

                _selectedItem = value;
                OnPropertyChanged();

                UpdateSelectedItemState();
            }
        }

        public string CurrentMp3FullPath
        {
            get => _currentMp3FullPath;
            private set
            {
                if (_currentMp3FullPath == value)
                {
                    return;
                }

                _currentMp3FullPath = value;
                OnPropertyChanged();
            }
        }

        public string PlaybackStatusText
        {
            get => _playbackStatusText;
            private set
            {
                if (_playbackStatusText == value)
                {
                    return;
                }

                _playbackStatusText = value;
                OnPropertyChanged();
            }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            private set
            {
                if (_isPlaying == value)
                {
                    return;
                }

                _isPlaying = value;
                OnPropertyChanged();
            }
        }

        public string CurrentTimeText
        {
            get => _currentTimeText;
            private set
            {
                if (_currentTimeText == value)
                {
                    return;
                }

                _currentTimeText = value;
                OnPropertyChanged();
            }
        }

        public string TotalDurationText
        {
            get => _totalDurationText;
            private set
            {
                if (_totalDurationText == value)
                {
                    return;
                }

                _totalDurationText = value;
                OnPropertyChanged();
            }
        }

        public double PlaybackProgressMaximum
        {
            get => _playbackProgressMaximum;
            private set
            {
                if (Math.Abs(_playbackProgressMaximum - value) < 0.001)
                {
                    return;
                }

                _playbackProgressMaximum = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 목적:
        /// 사용자가 드래그하는 재생 위치 슬라이더 값.
        /// </summary>
        public double SeekBarValue
        {
            get => _seekBarValue;
            set
            {
                if (Math.Abs(_seekBarValue - value) < 0.001)
                {
                    return;
                }

                _seekBarValue = value;
                OnPropertyChanged();

                if (_isSeekDragging)
                {
                    CurrentTimeText = FormatTime(TimeSpan.FromSeconds(_seekBarValue));
                }
            }
        }

        public int TotalCount => _allItems.Count;

        public int FilteredCount => FilteredItems.Count;

        public int ReadyCount => FilteredItems.Count(item =>
            string.Equals(item.Status, "가능", StringComparison.Ordinal));

        public int UnavailableCount => FilteredItems.Count(item =>
            string.Equals(item.Status, "불가능", StringComparison.Ordinal));

        public int CompletedCount => FilteredItems.Count(item =>
            string.Equals(item.Status, "완료", StringComparison.Ordinal));

        public ICommand ApplyFilterCommand { get; }

        public ICommand ResetFilterCommand { get; }

        public ICommand PlaySelectedCommand { get; }

        public ICommand StopPlaybackCommand { get; }

        public RecitingMusicViewModel()
        {
            _audioPlaybackService = new AudioPlaybackService();
            _audioPlaybackService.PlaybackStateChanged += OnPlaybackStateChanged;

            _recitingMusicDataService = new RecitingMusicDataService();

            _playbackTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _playbackTimer.Tick += OnPlaybackTimerTick;

            List<RecitingMusicItemViewModel> loadedItems = _recitingMusicDataService.LoadItems();

            _allItems = new ObservableCollection<RecitingMusicItemViewModel>(loadedItems);

            Courses = BuildCourseCollection(loadedItems);
            Days = BuildDayCollection(loadedItems);
            FilteredItems = new ObservableCollection<RecitingMusicItemViewModel>();

            ApplyFilterCommand = new RelayCommand(_ => ApplyFilter());
            ResetFilterCommand = new RelayCommand(_ => ResetFilter());
            PlaySelectedCommand = new RelayCommand(_ => PlaySelectedItem());
            StopPlaybackCommand = new RelayCommand(_ => StopPlayback());

            SelectedCourse = GetDefaultCourse();
            SelectedDay = GetDefaultDay();

            ApplyFilter();

            if (_allItems.Count == 0)
            {
                PlaybackStatusText = STATUS_NO_DATA;
            }
        }

        private static ObservableCollection<string> BuildCourseCollection(IEnumerable<RecitingMusicItemViewModel> items)
        {
            ObservableCollection<string> courses = new ObservableCollection<string>
            {
                "전체"
            };

            List<string> courseList = items
                .Select(item => item.Course)
                .Where(course => !string.IsNullOrWhiteSpace(course))
                .Distinct()
                .OrderBy(course => GetCourseSortOrder(course))
                .ThenBy(course => course, StringComparer.Ordinal)
                .ToList();

            foreach (string course in courseList)
            {
                courses.Add(course);
            }

            return courses;
        }

        private static ObservableCollection<string> BuildDayCollection(IEnumerable<RecitingMusicItemViewModel> items)
        {
            ObservableCollection<string> days = new ObservableCollection<string>
            {
                "전체"
            };

            List<string> dayList = items
                .Select(item => item.Day)
                .Where(day => !string.IsNullOrWhiteSpace(day))
                .Distinct()
                .OrderBy(day => GetDaySortOrder(day))
                .ThenBy(day => day, StringComparer.Ordinal)
                .ToList();

            foreach (string day in dayList)
            {
                days.Add(day);
            }

            return days;
        }

        private string GetDefaultCourse()
        {
            if (Courses.Contains("1과정"))
            {
                return "1과정";
            }

            return Courses.FirstOrDefault() ?? "전체";
        }

        private string GetDefaultDay()
        {
            if (Days.Contains("1일차"))
            {
                return "1일차";
            }

            return Days.FirstOrDefault() ?? "전체";
        }

        private void ApplyFilter()
        {
            List<RecitingMusicItemViewModel> matchedItems = _allItems
                .Where(item => IsCourseMatched(item) && IsDayMatched(item))
                .OrderBy(item => GetDaySortOrder(item.Day))
                .ThenBy(item => GetCourseSortOrder(item.Course))
                .ThenBy(item => item.Number)
                .ToList();

            FilteredItems.Clear();

            foreach (RecitingMusicItemViewModel item in matchedItems)
            {
                FilteredItems.Add(item);
            }

            SelectedItem = FilteredItems.FirstOrDefault();
            CurrentFilterText = BuildCurrentFilterText();

            NotifySummaryPropertiesChanged();
        }

        private void ResetFilter()
        {
            SelectedCourse = "전체";
            SelectedDay = "전체";

            ApplyFilter();
        }

        private bool IsCourseMatched(RecitingMusicItemViewModel item)
        {
            if (string.IsNullOrWhiteSpace(SelectedCourse) ||
                string.Equals(SelectedCourse, "전체", StringComparison.Ordinal))
            {
                return true;
            }

            int selectedCourseNumber = ParseDisplayNumber(SelectedCourse);
            int itemCourseNumber = ParseDisplayNumber(item.Course);

            if (selectedCourseNumber <= 0 || itemCourseNumber <= 0)
            {
                return string.Equals(item.Course, SelectedCourse, StringComparison.Ordinal);
            }

            return itemCourseNumber <= selectedCourseNumber;
        }

        private bool IsDayMatched(RecitingMusicItemViewModel item)
        {
            if (string.IsNullOrWhiteSpace(SelectedDay) ||
                string.Equals(SelectedDay, "전체", StringComparison.Ordinal))
            {
                return true;
            }

            int selectedDayNumber = ParseDisplayNumber(SelectedDay);
            int itemDayNumber = ParseDisplayNumber(item.Day);

            if (selectedDayNumber <= 0 || itemDayNumber <= 0)
            {
                return string.Equals(item.Day, SelectedDay, StringComparison.Ordinal);
            }

            return itemDayNumber == selectedDayNumber;
        }

        private string BuildCurrentFilterText()
        {
            string courseText = string.IsNullOrWhiteSpace(SelectedCourse) ? "전체" : SelectedCourse;
            string dayText = string.IsNullOrWhiteSpace(SelectedDay) ? "전체" : SelectedDay;

            return $"{courseText} / {dayText}";
        }

        private void UpdateSelectedItemState()
        {
            StopPlaybackTimer();
            ResetPlaybackProgress();

            if (_audioPlaybackService.IsPlaying)
            {
                _audioPlaybackService.Stop();
            }

            if (SelectedItem == null)
            {
                CurrentMp3FullPath = string.Empty;
                IsPlaying = false;
                UpdatePlaybackAvailabilityStatus();
                return;
            }

            CurrentMp3FullPath = ResolveMp3FullPath(
                SelectedItem.Course,
                SelectedItem.Day,
                SelectedItem.Mp3FileName);

            IsPlaying = false;
            UpdatePlaybackAvailabilityStatus();
        }

        private void UpdatePlaybackAvailabilityStatus()
        {
            if (SelectedItem == null)
            {
                if (_allItems.Count == 0)
                {
                    PlaybackStatusText = STATUS_NO_DATA;
                }
                else
                {
                    PlaybackStatusText = STATUS_SELECT_SONG;
                }

                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentMp3FullPath))
            {
                PlaybackStatusText = STATUS_UNAVAILABLE;
                return;
            }

            if (!File.Exists(CurrentMp3FullPath))
            {
                PlaybackStatusText = STATUS_UNAVAILABLE;
                return;
            }

            PlaybackStatusText = STATUS_PLAYABLE;
        }

        private void PlaySelectedItem()
        {
            if (SelectedItem == null)
            {
                PlaybackStatusText = STATUS_SELECT_FIRST;
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentMp3FullPath))
            {
                PlaybackStatusText = STATUS_UNAVAILABLE;
                return;
            }

            if (!File.Exists(CurrentMp3FullPath))
            {
                PlaybackStatusText = STATUS_UNAVAILABLE;
                return;
            }

            try
            {
                _audioPlaybackService.Play(CurrentMp3FullPath);
                PlaybackStatusText = $"재생 중: {SelectedItem.Reference}";
                IsPlaying = true;
                StartPlaybackTimer();
            }
            catch (Exception ex)
            {
                PlaybackStatusText = $"재생 실패: {ex.Message}";
                IsPlaying = false;
                StopPlaybackTimer();
                ResetPlaybackProgress();
            }
        }

        private void StopPlayback()
        {
            _audioPlaybackService.Stop();
            StopPlaybackTimer();
            ResetPlaybackProgress();
            IsPlaying = false;

            UpdatePlaybackAvailabilityStatus();
        }

        /// <summary>
        /// 목적:
        /// 다른 화면으로 이동하거나 View가 내려갈 때 현재 재생 중인 곡을 정지한다.
        /// </summary>
        public void StopPlaybackOnLeaveView()
        {
            StopPlayback();
        }

        public void BeginSeekDrag()
        {
            _isSeekDragging = true;
        }

        public void CommitSeek(double sliderValue)
        {
            double safeValue = sliderValue;

            if (safeValue < 0)
            {
                safeValue = 0;
            }

            if (safeValue > PlaybackProgressMaximum)
            {
                safeValue = PlaybackProgressMaximum;
            }

            _audioPlaybackService.Seek(TimeSpan.FromSeconds(safeValue));

            SeekBarValue = safeValue;
            CurrentTimeText = FormatTime(TimeSpan.FromSeconds(safeValue));

            _isSeekDragging = false;
        }

        private void StartPlaybackTimer()
        {
            _playbackTimer.Start();
        }

        private void StopPlaybackTimer()
        {
            _playbackTimer.Stop();
        }

        private void OnPlaybackTimerTick(object? sender, EventArgs e)
        {
            TimeSpan position = _audioPlaybackService.Position;
            TimeSpan duration = _audioPlaybackService.Duration;

            TotalDurationText = FormatTime(duration);

            double totalSeconds = duration.TotalSeconds;
            double currentSeconds = position.TotalSeconds;

            if (totalSeconds <= 0)
            {
                PlaybackProgressMaximum = 1;
                SeekBarValue = 0;
                CurrentTimeText = "00:00";
                return;
            }

            PlaybackProgressMaximum = totalSeconds;

            if (!_isSeekDragging)
            {
                SeekBarValue = Math.Min(currentSeconds, totalSeconds);
                CurrentTimeText = FormatTime(position);
            }
        }

        private void ResetPlaybackProgress()
        {
            CurrentTimeText = "00:00";
            TotalDurationText = "00:00";
            PlaybackProgressMaximum = 1;
            SeekBarValue = 0;
            _isSeekDragging = false;
        }

        private static string FormatTime(TimeSpan time)
        {
            if (time.TotalHours >= 1)
            {
                return time.ToString(@"hh\:mm\:ss");
            }

            return time.ToString(@"mm\:ss");
        }

        private void OnPlaybackStateChanged(object? sender, EventArgs e)
        {
            IsPlaying = _audioPlaybackService.IsPlaying;

            if (!IsPlaying)
            {
                StopPlaybackTimer();
                UpdatePlaybackAvailabilityStatus();
            }
        }

        private static string ResolveMp3FullPath(string course, string day, string mp3FileName)
        {
            if (string.IsNullOrWhiteSpace(mp3FileName))
            {
                return string.Empty;
            }

            string courseFolder = ConvertCourseToFolderName(course);
            string dayFolder = ConvertDayToFolderName(day);

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string outputPath = Path.Combine(
                baseDirectory,
                "Assets",
                "Audio",
                "RecitingMusic",
                courseFolder,
                dayFolder,
                mp3FileName);

            if (File.Exists(outputPath))
            {
                return outputPath;
            }

            string projectRootPath = Path.GetFullPath(Path.Combine(
                baseDirectory,
                @"..\..\..",
                "Assets",
                "Audio",
                "RecitingMusic",
                courseFolder,
                dayFolder,
                mp3FileName));

            if (File.Exists(projectRootPath))
            {
                return projectRootPath;
            }

            return string.Empty;
        }

        private static string ConvertCourseToFolderName(string course)
        {
            return course switch
            {
                "1과정" => "Course01",
                "2과정" => "Course02",
                "3과정" => "Course03",
                "4과정" => "Course04",
                _ => string.Empty
            };
        }

        private static string ConvertDayToFolderName(string day)
        {
            return day switch
            {
                "1일차" => "Day01",
                "2일차" => "Day02",
                "3일차" => "Day03",
                "4일차" => "Day04",
                "5일차" => "Day05",
                "6일차" => "Day06",
                _ => string.Empty
            };
        }

        private static int ParseDisplayNumber(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return -1;
            }

            string digits = new string(text.Where(char.IsDigit).ToArray());

            if (int.TryParse(digits, out int number))
            {
                return number;
            }

            return -1;
        }

        private static int GetCourseSortOrder(string? course)
        {
            int number = ParseDisplayNumber(course);
            return number > 0 ? number : int.MaxValue;
        }

        private static int GetDaySortOrder(string? day)
        {
            int number = ParseDisplayNumber(day);
            return number > 0 ? number : int.MaxValue;
        }

        private void NotifySummaryPropertiesChanged()
        {
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(FilteredCount));
            OnPropertyChanged(nameof(ReadyCount));
            OnPropertyChanged(nameof(UnavailableCount));
            OnPropertyChanged(nameof(CompletedCount));
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}