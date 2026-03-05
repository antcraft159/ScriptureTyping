using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ScriptureTyping.ViewModels.Games;

namespace ScriptureTyping.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private object? _currentContentViewModel;

        public string Title { get; } = "2026 상반기 사무엘학교";

        // TopBar/StatusBar 바인딩용 (XAML에서 쓰고 있으니 존재해야 함)
        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (Equals(_statusMessage, value)) return;
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
                if (Equals(_footerRight, value)) return;
                _footerRight = value;
                OnPropertyChanged();
            }
        }

        // 항상 고정 메뉴 VM
        public MainMenuViewModel MenuViewModel { get; }

        // 오른쪽 화면만 바뀌는 VM
        public object? CurrentContentViewModel
        {
            get => _currentContentViewModel;
            set
            {
                if (Equals(_currentContentViewModel, value)) return;
                _currentContentViewModel = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel()
        {
            // 메뉴가 화면을 바꾸려면 MainWindowVM의 네비게이션을 호출해야 하니까
            //    host(this)를 넘겨서 메서드 호출 방식으로 통일한다.
            MenuViewModel = new MainMenuViewModel(this, ExitApp);

            CurrentContentViewModel = null;

            StatusMessage = "준비됨";
            FooterRight = "v0.1";
        }

        /// <summary>
        /// 목적: 외부에서 오른쪽 콘텐츠 VM을 교체하는 공용 메서드.
        /// </summary>
        public void NavigateToContent(object vm)
        {
            CurrentContentViewModel = vm;
        }

        /// <summary>
        /// 목적: (호환) 다른 VM에서 NavigateTo(...) 형태로 호출해도 되도록 제공한다.
        /// </summary>
        public void NavigateTo(object vm)
        {
            NavigateToContent(vm);
        }

        /// <summary>
        /// 목적: (호환) 다른 VM에서 NavigateToContent(...)가 아니라 NavigateTo(...)를 쓰지 않아도 되게 제공한다.
        /// </summary>
        public void NavigateToContentViewModel(object vm)
        {
            NavigateToContent(vm);
        }

        /// <summary>
        /// 목적: 게임 허브로 이동
        /// </summary>
        public void NavigateToGamesHub()
        {
            NavigateToContent(new GamesHubViewModel(this));
        }

        /// <summary>
        /// 목적: 학습(코스 선택) 화면으로 이동
        /// </summary>
        public void NavigateToCourseSelect()
        {
            // CourseSelectViewModel 생성자가 Action<object?> 또는 Action<object> 형태를 요구하는 구조라면
            // 아래처럼 "NavigateToContent"로 연결해 주면 된다.
            NavigateToContent(new CourseSelectViewModel(o => NavigateToContent(o!)));
        }

        private void ExitApp()
        {
            System.Windows.Application.Current.Shutdown();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}