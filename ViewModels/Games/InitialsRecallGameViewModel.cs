using ScriptureTyping.Commands;

namespace ScriptureTyping.ViewModels.Games
{
    /// <summary>
    /// 목적: 초성/첫 글자 암송 화면의 상태/커맨드를 담당한다.
    /// 현재: 기본 화면 틀 + 뒤로가기만 제공.
    /// </summary>
    public sealed class InitialsRecallGameViewModel : BaseViewModel
    {
        private readonly MainWindowViewModel _host;

        public string Title => "초성/첫 글자 암송";
        public RelayCommand BackCommand { get; }

        public InitialsRecallGameViewModel(MainWindowViewModel host)
        {
            _host = host;
            BackCommand = new RelayCommand(_ => _host.NavigateToGamesHub());
        }
    }
}