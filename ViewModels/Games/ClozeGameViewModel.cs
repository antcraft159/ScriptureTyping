using ScriptureTyping.Commands;

namespace ScriptureTyping.ViewModels.Games
{
    /// <summary>
    /// 목적: 빈칸 채우기 게임 화면의 상태/커맨드를 담당한다.
    /// 현재: 기본 화면 틀 + 뒤로가기만 제공. (로직은 다음 단계에서 구현)
    /// </summary>
    public sealed class ClozeGameViewModel : BaseViewModel
    {
        private readonly MainWindowViewModel _host;

        public string Title => "구절 빈칸 채우기";
        public RelayCommand BackCommand { get; }

        public ClozeGameViewModel(MainWindowViewModel host)
        {
            _host = host;
            BackCommand = new RelayCommand(_ => _host.NavigateToGamesHub(), _ => true);
        }
    }
}