using ScriptureTyping.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptureTyping.ViewModels.Games.VerseMatch
{
    /// <summary>
    /// 목적:
    /// 구절 짝 맞추기 게임에서 선택된 과정/일차 기준으로 사용할 구절 목록을 반환한다.
    /// </summary>
    public sealed class VerseMatchSelectionService
    {
        private const string ALL_DAY_TEXT = "전일차";

        /// <summary>
        /// 목적:
        /// 선택된 과정/일차에 맞는 Verse 목록을 반환한다.
        /// 데이터가 없으면 기본값(1과정 1일차)로 대체한다.
        /// </summary>
        /// <param name="selectedCourse">선택된 과정 텍스트</param>
        /// <param name="selectedDay">선택된 일차 텍스트</param>
        /// <returns>게임에 사용할 Verse 목록</returns>
        public IReadOnlyList<Verse> GetSourceVerses(string? selectedCourse, string? selectedDay)
        {
            int courseNo = ParseCourseNo(selectedCourse);
            int dayIndex = ParseDayIndex(selectedDay);

            IReadOnlyList<Verse> selectedVerses = dayIndex == 0
                ? BuildAllDayVerseList(courseNo)
                : VerseCatalog.GetAccumulated(courseNo, dayIndex);

            List<Verse> result = FilterValidVerses(selectedVerses);

            if (result.Count > 0)
            {
                return result;
            }

            IReadOnlyList<Verse> fallback = VerseCatalog.GetAccumulated(1, 1);
            return FilterValidVerses(fallback);
        }

        /// <summary>
        /// 목적:
        /// 전일차 선택 시 전체 구절 목록을 만든다.
        /// </summary>
        /// <param name="courseNo">과정 번호</param>
        /// <returns>과정 전체 일차에서 누적된 Verse 목록</returns>
        private static IReadOnlyList<Verse> BuildAllDayVerseList(int courseNo)
        {
            List<Verse> all = new List<Verse>();

            for (int day = 1; day <= VerseCatalog.MAX_DAY; day++)
            {
                IReadOnlyList<Verse> verses = VerseCatalog.GetAccumulated(courseNo, day);
                all.AddRange(verses);
            }

            return all
                .Where(x => x is not null)
                .GroupBy(x => x.Ref)
                .Select(g => g.First())
                .ToList();
        }

        /// <summary>
        /// 목적:
        /// 실제 게임에 사용할 수 있는 Verse만 필터링한다.
        /// </summary>
        /// <param name="verses">원본 Verse 목록</param>
        /// <returns>유효한 Verse 목록</returns>
        private static List<Verse> FilterValidVerses(IEnumerable<Verse> verses)
        {
            List<Verse> result = new List<Verse>();

            foreach (Verse verse in verses)
            {
                if (verse is null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(verse.Ref) || string.IsNullOrWhiteSpace(verse.Text))
                {
                    continue;
                }

                result.Add(verse);
            }

            return result;
        }

        /// <summary>
        /// 목적:
        /// "1과정" 같은 텍스트에서 숫자만 파싱한다.
        /// </summary>
        /// <param name="courseText">과정 텍스트</param>
        /// <returns>과정 번호</returns>
        private static int ParseCourseNo(string? courseText)
        {
            if (string.IsNullOrWhiteSpace(courseText))
            {
                return 1;
            }

            string digits = new string(courseText.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out int number) ? number : 1;
        }

        /// <summary>
        /// 목적:
        /// "전일차" 또는 "3일차" 텍스트를 day index로 변환한다.
        /// </summary>
        /// <param name="dayText">일차 텍스트</param>
        /// <returns>day index. 전일차는 0</returns>
        private static int ParseDayIndex(string? dayText)
        {
            if (string.IsNullOrWhiteSpace(dayText))
            {
                return 1;
            }

            if (string.Equals(dayText, ALL_DAY_TEXT, StringComparison.Ordinal))
            {
                return 0;
            }

            string digits = new string(dayText.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out int day) ? day : 1;
        }
    }
}