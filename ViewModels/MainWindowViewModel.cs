using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ScriptureTyping.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private object? _currentContentViewModel;

        public string Title { get; } = "2026 상반기 사무엘학교";

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
            MenuViewModel = new MainMenuViewModel(NavigateToContent, ExitApp);

            // 앱 처음 켰을 때 오른쪽은 비워두거나 기본 화면 지정
            CurrentContentViewModel = null;
        }

        private void NavigateToContent(object vm)
        {
            CurrentContentViewModel = vm;
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