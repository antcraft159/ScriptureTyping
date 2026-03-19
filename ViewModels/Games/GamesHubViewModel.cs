using ScriptureTyping.Commands;
using ScriptureTyping.ViewModels.Games;
using ScriptureTyping.ViewModels.Games.VerseMatch;
using System.Collections.ObjectModel;

namespace ScriptureTyping.ViewModels.Games
{
    public sealed class GamesHubViewModel : BaseViewModel
    {
        public ObservableCollection<GameCardViewModel> Games { get; } = new();

        public GamesHubViewModel(MainWindowViewModel host)
        {
            Games.Add(new GameCardViewModel(
                title: "빈칸 폭격전",
                description: "사라진 말씀 조각을 찾아라. 빈칸을 채우며 정답을 완성하는 스피드 암송 배틀",
                startCommand: new RelayCommand(_ => host.NavigateTo(new ClozeGameViewModel(host)))
            ));

            Games.Add(new GameCardViewModel(
                title: "말씀 순서 챌린지",
                description: "뒤섞인 말씀 조각을 순서대로 맞춰라. 흐트러진 문장을 바르게 정렬하는 퍼즐 챌린지",
                startCommand: new RelayCommand(
                    _ => host.NavigateTo(
                        new ScriptureTyping.ViewModels.Games.WordOrder.WordOrderGameViewModel(host)))
            ));

            Games.Add(new GameCardViewModel(
                title: "짝꿍 카드 대작전",
                description: "장절 카드와 본문 카드를 기억해서 연결하라. 진짜 짝꿍을 찾아내는 두뇌 매칭 게임",
                startCommand: new RelayCommand(
                    _ => host.NavigateTo(
                        new ScriptureTyping.ViewModels.Games.VerseMatch.VerseMatchGameViewModel(host)))
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