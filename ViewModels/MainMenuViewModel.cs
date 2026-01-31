// MainMenuViewModel.cs
using ScriptureTyping.Commands;
using System;
using System.Windows.Input;

namespace ScriptureTyping.ViewModels
{
    public sealed class MainMenuViewModel
    {
        private readonly Action<object> _navigate;
        private readonly Action _exit;

        public ICommand StartLearningCommand { get; }
        public ICommand GamesCommand { get; }
        public ICommand RecordsCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand ExitCommand { get; }

        public MainMenuViewModel(Action<object> navigate, Action exit)
        {
            if (navigate == null)
            {
                throw new ArgumentNullException(nameof(navigate));
            }

            if (exit == null)
            {
                throw new ArgumentNullException(nameof(exit));
            }

            _navigate = navigate;
            _exit = exit;

            StartLearningCommand = new RelayCommand(
                _ => NavigateToCourseSelect(),
                _ => true);

            GamesCommand = new RelayCommand(
                _ => OpenPlaceholder("게임", "GameHub 화면으로 연결하면 됨."),
                _ => true);

            RecordsCommand = new RelayCommand(
                _ => OpenPlaceholder("기록/통계", "Progress/Stats 화면으로 연결하면 됨."),
                _ => true);

            SettingsCommand = new RelayCommand(
                _ => OpenPlaceholder("설정", "settings.json 로드/저장으로 연결하면 됨."),
                _ => true);

            ExitCommand = new RelayCommand(
                _ => Exit(),
                _ => true);
        }

        private void NavigateToCourseSelect()
        {
            _navigate(new CourseSelectViewModel(_navigate));
        }

        private void Exit()
        {
            _exit();
        }

        private void OpenPlaceholder(string title, string message)
        {
            _navigate(new PlaceholderViewModel(title, message));
        }
    }
}
