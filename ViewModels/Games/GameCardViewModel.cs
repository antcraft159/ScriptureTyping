using System.Windows.Input;

namespace ScriptureTyping.ViewModels.Games
{
    /// <summary>
    /// 목적: 게임 허브에서 보여줄 "게임 카드 1개"의 데이터를 표현한다.
    /// 구성: 제목/설명/시작 커맨드.
    /// 이유: 카드 UI를 ItemsControl로 뿌릴 때 바인딩 구조가 단순해진다.
    /// </summary>
    public sealed class GameCardViewModel
    {
        public string Title { get; }
        public string Description { get; }
        public ICommand StartCommand { get; }

        public GameCardViewModel(string title, string description, ICommand startCommand)
        {
            Title = title;
            Description = description;
            StartCommand = startCommand;
        }
    }
}