using ScriptureTyping.Data;
using ScriptureTyping.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.WordOrder
{
    /// <summary>
    /// 목적:
    /// 순서 맞추기 게임에서 과정/일차 선택 UI와 선택 결과를 관리하는 서비스이다.
    ///
    /// 주요 역할:
    /// - 과정 목록 생성
    /// - 일차 목록 생성
    /// - SelectionContext 기본값 반영
    /// - 선택값 검증
    /// - 선택된 조건에 맞는 Verse 목록 반환
    /// - SelectionContext 저장
    ///
    /// 입력:
    /// - SelectionContext
    /// - 선택된 과정 문자열 (예: "1과정")
    /// - 선택된 일차 문자열 (예: "3일차", "전일차")
    ///
    /// 출력:
    /// - 과정 목록
    /// - 일차 목록
    /// - 현재 선택 상태
    /// - 선택 조건에 맞는 Verse 목록
    ///
    /// 주의사항:
    /// - 전일차는 내부적으로 0으로 취급한다.
    /// - 전일차 선택 시 해당 과정의 모든 일차 누적 구절을 합친 뒤 Ref 기준으로 중복 제거한다.
    /// - UI 문자열 형식이 바뀌면 ParseCourseNo / ParseDayNo도 같이 수정해야 한다.
    /// </summary>
    public sealed class WordOrderSelectionService
    {
        public const string AllDayText = "전일차";
        public const int AllDayIndex = 0;

        private readonly SelectionContext _selectionContext;

        public WordOrderSelectionService(SelectionContext selectionContext)
        {
            _selectionContext = selectionContext ?? throw new ArgumentNullException(nameof(selectionContext));

            Courses = new ObservableCollection<string>();
            Days = new ObservableCollection<string>();

            InitializeOptions();
            ApplySelectionFromContextOrDefault();
        }

        /// <summary>
        /// 과정 목록
        /// </summary>
        public ObservableCollection<string> Courses { get; }

        /// <summary>
        /// 일차 목록
        /// </summary>
        public ObservableCollection<string> Days { get; }

        /// <summary>
        /// 현재 선택된 과정
        /// </summary>
        public string? SelectedCourse { get; private set; }

        /// <summary>
        /// 현재 선택된 일차
        /// </summary>
        public string? SelectedDay { get; private set; }

        /// <summary>
        /// 선택 오류 메시지
        /// </summary>
        public string SelectionError { get; private set; } = string.Empty;

        /// <summary>
        /// 과정/일차 선택 UI 목록을 초기화한다.
        /// </summary>
        private void InitializeOptions()
        {
            Courses.Clear();
            for (int course = 1; course <= VerseCatalog.MAX_COURSE; course++)
            {
                Courses.Add($"{course}과정");
            }

            Days.Clear();
            for (int day = 1; day <= VerseCatalog.MAX_DAY; day++)
            {
                Days.Add($"{day}일차");
            }

            Days.Add(AllDayText);
        }

        /// <summary>
        /// SelectionContext에 저장된 값이 있으면 반영하고, 없으면 기본값을 사용한다.
        /// </summary>
        public void ApplySelectionFromContextOrDefault()
        {
            if (_selectionContext.HasSelection)
            {
                SelectedCourse = NormalizeCourse(_selectionContext.SelectedCourseId);
                SelectedDay = NormalizeDay(_selectionContext.SelectedDayIndex);
                SelectionError = string.Empty;
                return;
            }

            SelectedCourse = Courses.FirstOrDefault();
            SelectedDay = Days.FirstOrDefault();
            SelectionError = string.Empty;
        }

        /// <summary>
        /// 현재 선택값을 변경한다.
        /// </summary>
        public void SetSelection(string? selectedCourse, string? selectedDay)
        {
            SelectedCourse = selectedCourse;
            SelectedDay = selectedDay;
            SelectionError = string.Empty;
        }

        /// <summary>
        /// 현재 선택값이 유효한지 검사한다.
        /// </summary>
        public bool CanApplySelection()
        {
            return !string.IsNullOrWhiteSpace(SelectedCourse)
                && !string.IsNullOrWhiteSpace(SelectedDay);
        }

        /// <summary>
        /// 현재 선택 조건으로 Verse 목록을 만든다.
        /// 성공하면 SelectionContext에도 반영한다.
        /// </summary>
        public bool TryBuildSelection(out List<Verse> verses)
        {
            verses = new List<Verse>();

            if (!CanApplySelection())
            {
                SelectionError = "과정/일차를 선택해 주세요.";
                return false;
            }

            int courseNo = ParseCourseNo(SelectedCourse!);
            int dayIndex = SelectedDay == AllDayText
                ? AllDayIndex
                : ParseDayNo(SelectedDay!);

            verses = BuildVerseList(courseNo, SelectedDay!).ToList();

            if (verses.Count == 0)
            {
                SelectionError = "해당 선택에 구절이 없습니다.";
                return false;
            }

            SaveSelectionContext(courseNo, dayIndex, verses);
            SelectionError = string.Empty;
            return true;
        }

        /// <summary>
        /// 선택된 과정/일차에 따라 게임용 Verse 목록을 만든다.
        /// </summary>
        public IReadOnlyList<Verse> BuildVerseList(int course, string selectedDay)
        {
            if (string.Equals(selectedDay, AllDayText, StringComparison.Ordinal))
            {
                List<Verse> allVerses = new();

                for (int day = 1; day <= VerseCatalog.MAX_DAY; day++)
                {
                    IReadOnlyList<Verse> accumulatedVerses = VerseCatalog.GetAccumulated(course, day);
                    allVerses.AddRange(accumulatedVerses);
                }

                return allVerses
                    .GroupBy(verse => verse.Ref)
                    .Select(group => group.First())
                    .ToList();
            }

            int dayNumber = ParseDayNo(selectedDay);
            return VerseCatalog.GetAccumulated(course, dayNumber).ToList();
        }

        /// <summary>
        /// 현재 선택 결과를 SelectionContext에 저장한다.
        /// </summary>
        private void SaveSelectionContext(int courseNo, int dayIndex, IReadOnlyList<Verse> verses)
        {
            List<VerseItem> items = verses
                .Select(verse => new VerseItem
                {
                    Ref = verse.Ref,
                    Text = verse.Text
                })
                .ToList();

            _selectionContext.SetSelection($"{courseNo}과정", dayIndex, items);
        }

        /// <summary>
        /// SelectionContext의 과정 값을 UI 문자열 형식으로 정규화한다.
        /// </summary>
        private string NormalizeCourse(string? selectedCourseId)
        {
            if (string.IsNullOrWhiteSpace(selectedCourseId))
            {
                return Courses.FirstOrDefault() ?? "1과정";
            }

            string normalized = selectedCourseId!.Trim();

            if (Courses.Contains(normalized))
            {
                return normalized;
            }

            int courseNo = ParseCourseNo(normalized);
            string courseText = $"{courseNo}과정";

            return Courses.Contains(courseText)
                ? courseText
                : Courses.FirstOrDefault() ?? courseText;
        }

        /// <summary>
        /// SelectionContext의 일차 값을 UI 문자열 형식으로 정규화한다.
        /// </summary>
        private string NormalizeDay(int? selectedDayIndex)
        {
            int dayIndex = selectedDayIndex ?? 1;

            if (dayIndex == AllDayIndex)
            {
                return AllDayText;
            }

            string dayText = $"{dayIndex}일차";

            return Days.Contains(dayText)
                ? dayText
                : Days.FirstOrDefault() ?? "1일차";
        }

        /// <summary>
        /// "1과정" 같은 문자열에서 과정 번호를 추출한다.
        /// </summary>
        public static int ParseCourseNo(string courseText)
        {
            if (string.IsNullOrWhiteSpace(courseText))
            {
                return 1;
            }

            string digits = new string(courseText.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out int number) ? number : 1;
        }

        /// <summary>
        /// "3일차" 같은 문자열에서 일차 번호를 추출한다.
        /// </summary>
        public static int ParseDayNo(string dayText)
        {
            if (string.IsNullOrWhiteSpace(dayText))
            {
                return 1;
            }

            string digits = new string(dayText.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out int number) ? number : 1;
        }
    }
}