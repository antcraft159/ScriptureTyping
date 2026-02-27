using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ScriptureTyping.ViewModels
{
    /// <summary>
    /// 목적: 모든 ViewModel이 공통으로 사용하는 PropertyChanged 구현을 제공한다.
    /// 이유: 게임 VM이 여러 개 생기면 INotifyPropertyChanged 반복 작성이 너무 많아진다.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
}