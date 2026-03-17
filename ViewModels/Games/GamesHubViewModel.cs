using ScriptureTyping.Commands;
using System.Collections.ObjectModel;

namespace ScriptureTyping.ViewModels.Games
{
    public sealed class GamesHubViewModel : BaseViewModel
    {
        public ObservableCollection<GameCardViewModel> Games { get; } = new();

        public GamesHubViewModel(MainWindowViewModel host)
        {
            Games.Add(new GameCardViewModel(
                title: "구절 빈칸 채우기",
                description: "구절의 일부가 ____로 비어있다. 입력해서 완성한다.",
                startCommand: new RelayCommand(_ => host.NavigateTo(new ClozeGameViewModel(host)))
            ));

            Games.Add(new GameCardViewModel(
                title: "순서 맞추기",
                description: "단어/구절 조각이 섞여있다. 올바른 순서를 맞춘다.",
                startCommand: new RelayCommand(
                    _ => host.NavigateTo(
                        new ScriptureTyping.ViewModels.Games.WordOrder.WordOrderGameViewModel(host)))
            ));

            Games.Add(new GameCardViewModel(
                title: "스피드 타이핑",
                description: "제한 시간 내 정확도/속도로 점수 획득.",
                startCommand: new RelayCommand(_ => host.NavigateTo(new SpeedTypingGameViewModel(host)))
            ));

            Games.Add(new GameCardViewModel(
                title: "오타 찾기",
                description: "일부가 틀린 구절을 보여준다. 틀린 부분을 찾아 고친다.",
                startCommand: new RelayCommand(_ => host.NavigateTo(new MistakeHuntGameViewModel(host)))
            ));

            Games.Add(new GameCardViewModel(
                title: "초성/첫 글자 암송",
                description: "초성(또는 첫 글자)만 보고 문장을 복원한다.",
                startCommand: new RelayCommand(_ => host.NavigateTo(new InitialsRecallGameViewModel(host)))
            ));
        }
    }
}