using System;

namespace ScriptureTyping.ViewModels.Games
{
    public sealed partial class WordOrderGameViewModel
    {
        private readonly MainWindowViewModel _host;

        public WordOrderGameViewModel(MainWindowViewModel host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }
    }
}