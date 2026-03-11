using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ScriptureTyping.ViewModels.Games.Cloze.Models
{
    /// <summary>
    /// 목적:
    /// 빈칸 하나에 대응되는 보기 그룹 상태를 관리한다.
    ///
    /// 사용 예:
    /// - 1번째 빈칸 보기 6개
    /// - 2번째 빈칸 보기 6개
    /// - ...
    /// - N번째 빈칸에서 사용자가 선택한 값 저장
    /// </summary>
    public sealed class ClozeChoiceGroupItem : INotifyPropertyChanged
    {
        private string? _selectedChoice;

        /// <summary>
        /// 몇 번째 빈칸인지 나타낸다. 0부터 시작한다.
        /// </summary>
        public int BlankIndex { get; init; }

        /// <summary>
        /// 화면에 표시할 제목이다.
        /// 예: "1번째 빈칸 보기"
        /// </summary>
        public string Title => $"{BlankIndex + 1}번째 빈칸 보기";

        /// <summary>
        /// 해당 빈칸에 표시할 보기 목록이다.
        /// </summary>
        public ObservableCollection<string> Choices { get; } = new ObservableCollection<string>();

        /// <summary>
        /// 사용자가 현재 선택한 보기 값이다.
        /// </summary>
        public string? SelectedChoice
        {
            get => _selectedChoice;
            set
            {
                if (_selectedChoice == value)
                {
                    return;
                }

                _selectedChoice = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}