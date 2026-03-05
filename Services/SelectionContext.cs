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

        private readonly ObservableCollection<VerseItem> _selectedVerses = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>선택된 과정 ID</summary>
        public string? SelectedCourseId
        {
            get => _selectedCourseId;
            private set
            {
                if (_selectedCourseId == value) return;
                _selectedCourseId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelection));
            }
        }

        /// <summary>선택된 일차 인덱스</summary>
        public int? SelectedDayIndex
        {
            get => _selectedDayIndex;
            private set
            {
                if (_selectedDayIndex == value) return;
                _selectedDayIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelection));
            }
        }

        /// <summary>
        /// 선택된 구절 목록(읽기 전용 뷰).
        /// UI에서 바인딩해도 안전하게 읽기만 가능.
        /// </summary>
        public ReadOnlyObservableCollection<VerseItem> SelectedVerses { get; }

        public int SelectedVerseCount => _selectedVerses.Count;

        public bool HasSelection =>
            !string.IsNullOrWhiteSpace(SelectedCourseId)
            && SelectedDayIndex.HasValue
            && SelectedVerseCount > 0;

        public SelectionContext()
        {
            SelectedVerses = new ReadOnlyObservableCollection<VerseItem>(_selectedVerses);
            _selectedVerses.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(SelectedVerseCount));
                OnPropertyChanged(nameof(HasSelection));
            };
        }

        /// <summary>
        /// 선택 상태를 한 번에 원자적으로 세팅한다.
        /// </summary>
        public void SetSelection(string courseId, int dayIndex, IReadOnlyList<VerseItem> verses)
        {
            if (string.IsNullOrWhiteSpace(courseId))
            {
                throw new ArgumentException("courseId is null or empty.", nameof(courseId));
            }

            if (dayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dayIndex), "dayIndex must be >= 0.");
            }

            SelectedCourseId = courseId;
            SelectedDayIndex = dayIndex;

            _selectedVerses.Clear();
            if (verses != null)
            {
                for (int i = 0; i < verses.Count; i++)
                {
                    _selectedVerses.Add(verses[i]);
                }
            }
        }

        public void Clear()
        {
            SelectedCourseId = null;
            SelectedDayIndex = null;
            _selectedVerses.Clear();
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public sealed class VerseItem
    {
        public string Ref { get; init; } = string.Empty;
        public string Text { get; init; } = string.Empty;
    }
}