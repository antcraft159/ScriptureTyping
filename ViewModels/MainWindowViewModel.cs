using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ScriptureTyping.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private object? _currentViewModel;

        public string Title { get; } = "2026 상반기 사무엘학교";

        public object? CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                if (Equals(_currentViewModel, value)) return;
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel()
        {
            CurrentViewModel = new MainMenuViewModel(NavigateTo, ExitApp);
        }

        private void NavigateTo(object vm)
        {
            CurrentViewModel = vm;
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
