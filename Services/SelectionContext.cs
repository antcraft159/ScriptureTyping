using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ScriptureTyping.Services
{
    /// <summary>
    /// 목적: 앱 전체에서 공유되는 "과정/일차 선택 + 해당 구절 리스트" 상태 저장소.
    ///       학습/게임 어디서 선택하든 여기로 모아 동기화한다.
    /// </summary>
    public sealed class SelectionContext : INotifyPropertyChanged
    {
        private string? _selectedCourseId;
        private int? _selectedDayIndex;

        private IReadOnlyList<VerseItem> _selectedVerses = Array.Empty<VerseItem>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? SelectedCourseId
        {
            get => _selectedCourseId;
            set
            {
                if (_selectedCourseId == value) return;
                _selectedCourseId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelection));
            }
        }

        public int? SelectedDayIndex
        {
            get => _selectedDayIndex;
            set
            {
                if (_selectedDayIndex == value) return;
                _selectedDayIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelection));
            }
        }

        public IReadOnlyList<VerseItem> SelectedVerses
        {
            get => _selectedVerses;
            private set
            {
                _selectedVerses = value ?? Array.Empty<VerseItem>();
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedVerseCount));
                OnPropertyChanged(nameof(HasSelection));
            }
        }

        public int SelectedVerseCount => _selectedVerses.Count;

        public bool HasSelection =>
            !string.IsNullOrWhiteSpace(SelectedCourseId)
            && SelectedDayIndex.HasValue
            && SelectedVerseCount > 0;

        public void SetSelection(string courseId, int dayIndex, IReadOnlyList<VerseItem> verses)
        {
            SelectedCourseId = courseId;
            SelectedDayIndex = dayIndex;
            SelectedVerses = verses ?? Array.Empty<VerseItem>();
        }

        public void Clear()
        {
            SelectedCourseId = null;
            SelectedDayIndex = null;
            SelectedVerses = Array.Empty<VerseItem>();
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// TODO: 너 프로젝트의 실제 구절 타입으로 바꿔라.
    /// </summary>
    public sealed class VerseItem
    {
        public string Ref { get; init; } = string.Empty;
        public string Text { get; init; } = string.Empty;
    }
}