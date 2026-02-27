using System;
using System.Windows.Input;

namespace ScriptureTyping.Commands
{
    public sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// 목적: canExecute 없이도 커맨드를 만들 수 있게 한다(기본 true).
        /// 이유: 대부분 버튼은 항상 실행 가능이라 canExecute를 매번 쓰면 귀찮다.
        /// </summary>
        public RelayCommand(Action<object?> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}