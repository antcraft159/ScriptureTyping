using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ScriptureTyping.ViewModels.Games;
using ScriptureTyping.ViewModels.RecitingMusic;

namespace ScriptureTyping.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private object? _currentContentViewModel;

        public string Title { get; } = "2026 상반기 사무엘학교";

        private string _statusMessage = string.Empty;
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

        private string _footerRight = string.Empty;
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

        public MainMenuViewModel MenuViewModel { get; }

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
            }
        }

        public MainWindowViewModel()
        {
            MenuViewModel = new MainMenuViewModel(this, ExitApp);

            CurrentContentViewModel = null;

            StatusMessage = "가능";
            FooterRight = "v0.1";
        }

        /// <summary>
        /// 목적:
        /// 외부에서 오른쪽 콘텐츠 VM을 교체한다.
        /// 화면 전환 전에 현재 화면이 암송 동요 화면이면 재생 중인 오디오를 정지한다.
        /// </summary>
        public void NavigateToContent(object vm)
        {
            StopCurrentContentIfNeeded();
            CurrentContentViewModel = vm;
        }

        /// <summary>
        /// 목적:
        /// (호환) 다른 VM에서 NavigateTo(...) 형태로 호출해도 동작하도록 제공한다.
        /// </summary>
        public void NavigateTo(object vm)
        {
            NavigateToContent(vm);
        }

        /// <summary>
        /// 목적:
        /// (호환) 다른 VM에서 NavigateToContentViewModel(...) 형태로 호출해도 동작하도록 제공한다.
        /// </summary>
        public void NavigateToContentViewModel(object vm)
        {
            NavigateToContent(vm);
        }

        /// <summary>
        /// 목적:
        /// 게임 허브로 이동한다.
        /// </summary>
        public void NavigateToGamesHub()
        {
            NavigateToContent(new GamesHubViewModel(this));
        }

        /// <summary>
        /// 목적:
        /// 학습(코스 선택) 화면으로 이동한다.
        /// </summary>
        public void NavigateToCourseSelect()
        {
            NavigateToContent(new CourseSelectViewModel(o => NavigateToContent(o!)));
        }

        /// <summary>
        /// 목적:
        /// 현재 화면이 암송 동요 화면이면 재생 중인 노래를 정지한다.
        /// </summary>
        private void StopCurrentContentIfNeeded()
        {
            if (CurrentContentViewModel is RecitingMusicViewModel recitingMusicViewModel)
            {
                recitingMusicViewModel.StopPlaybackOnLeaveView();
            }
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
}