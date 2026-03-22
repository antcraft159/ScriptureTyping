using ScriptureTyping.Commands;
using ScriptureTyping.ViewModels.RecitingMusic;
using System;
using System.Windows.Input;

namespace ScriptureTyping.ViewModels
{
    public sealed class MainMenuViewModel
    {
        private readonly MainWindowViewModel _host;
        private readonly Action _exit;

        public ICommand StartLearningCommand { get; }
        public ICommand GamesCommand { get; }
        public ICommand RecordsCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand ExitCommand { get; }

        public MainMenuViewModel(MainWindowViewModel host, Action exit)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _exit = exit ?? throw new ArgumentNullException(nameof(exit));

            StartLearningCommand = new RelayCommand(_ => _host.NavigateToCourseSelect(), _ => true);

            GamesCommand = new RelayCommand(_ => _host.NavigateToGamesHub(), _ => true);

            RecordsCommand = new RelayCommand(
                _ => _host.NavigateToContent(new RecitingMusicViewModel()),
                _ => true);

            SettingsCommand = new RelayCommand(
                _ => _host.NavigateToContent(new PlaceholderViewModel("설정", "settings.json 로드/저장으로 연결하면 됨.")),
                _ => true);

            ExitCommand = new RelayCommand(_ => _exit(), _ => true);
        }
    }
}