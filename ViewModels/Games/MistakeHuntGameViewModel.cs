using ScriptureTyping.Commands;

namespace ScriptureTyping.ViewModels.Games
{
    /// <summary>
    /// 목적: 오타 찾기 게임 화면의 상태/커맨드를 담당한다.
    /// 현재: 기본 화면 틀 + 뒤로가기만 제공.
    /// </summary>
    public sealed class MistakeHuntGameViewModel : BaseViewModel
    {
        private readonly MainWindowViewModel _host;

        public string Title => "오타 찾기";
        public RelayCommand BackCommand { get; }

        public MistakeHuntGameViewModel(MainWindowViewModel host)
        {
            _host = host;
            BackCommand = new RelayCommand(_ => _host.NavigateToGamesHub());
        }
    }
}