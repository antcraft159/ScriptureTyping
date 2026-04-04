using ScriptureTyping.Data;
using ScriptureTyping.Shared.Infrastructure;
using ScriptureTyping.Shared.Models.Schedule;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ScriptureTyping.Services
{
    public sealed class ScheduleService
    {
        private const int ScheduleYear = 2026;
        private static readonly TimeSpan NextScheduleNoticeThreshold = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan OpeningCountdownThreshold = TimeSpan.FromDays(7);

        private readonly List<ScheduleItem> _schedules;

        public ScheduleService()
        {
            _schedules = LoadSchedules();
        }

        public IReadOnlyList<ScheduleItem> GetTodaySchedules(DateTime now)
        {
            return GetResolvedSchedules()
                .Where(item => item.Start.Date == now.Date)
                .OrderBy(item => item.Start)
                .Select(item => item.Item)
                .ToList();
        }

        public ScheduleItem? GetCurrentSchedule(DateTime now)
        {
            ResolvedScheduleItem? current = GetCurrentOccurrence(now);
            return current?.Item;
        }

        public ScheduleItem? GetNextSchedule(DateTime now)
        {
            ResolvedScheduleItem? next = GetNextOccurrence(now);
            return next?.Item;
        }

        public bool IsTodayScheduleFinished(DateTime now)
        {
            List<ResolvedScheduleItem> todaySchedules = GetResolvedSchedules()
                .Where(item => item.Start.Date == now.Date)
                .OrderBy(item => item.Start)
                .ToList();

            if (todaySchedules.Count == 0)
            {
                return false;
            }

            return now >= todaySchedules.Last().End;
        }

        public bool IsAllSchedulesFinished(DateTime now)
        {
            if (!TryGetLastScheduleEndDateTime(out DateTime lastEnd))
            {
                return false;
            }

            return now >= lastEnd;
        }

        public bool IsBeforeFirstSchedule(DateTime now)
        {
            if (!TryGetFirstScheduleStartDateTime(out DateTime firstStart))
            {
                return false;
            }

            return now < firstStart;
        }

        public bool ShouldShowOpeningCountdown(DateTime now)
        {
            if (!TryGetFirstScheduleStartDateTime(out DateTime firstStart))
            {
                return false;
            }

            if (now >= firstStart)
            {
                return false;
            }

            TimeSpan remaining = firstStart - now;
            return remaining <= OpeningCountdownThreshold;
        }

        public string BuildOpeningCountdownMessage(DateTime now)
        {
            if (!TryGetFirstScheduleStartDateTime(out DateTime firstStart))
            {
                return GetFinishedPrimaryMessage();
            }

            TimeSpan remaining = firstStart - now;

            if (remaining < TimeSpan.Zero)
            {
                remaining = TimeSpan.Zero;
            }

            return $"사무엘학교 개강일이 {FormatDayHourMinuteCountdown(remaining)} 남았습니다";
        }

        public string GetFinishedPrimaryMessage()
        {
            return "정말 수고하셨습니다";
        }

        public string GetFinishedSecondaryMessage()
        {
            return "다음 사무엘학교에서 뵙겠습니다";
        }

        public string GetPreparationSecondaryMessage()
        {
            return "기도하는 마음으로 준비해주시길 바랍니다";
        }

        public string BuildNextScheduleMessage(DateTime now)
        {
            ResolvedScheduleItem? nextSchedule = GetNextOccurrence(now);

            if (nextSchedule is null)
            {
                return "다음 일정: 오늘 남은 일정 없음";
            }

            TimeSpan remaining = nextSchedule.Start - now;

            if (remaining > TimeSpan.Zero && remaining <= NextScheduleNoticeThreshold)
            {
                string remainingText = FormatCountdown(remaining);
                return $"{remainingText} 후 다음일정은 {nextSchedule.Item.Title}입니다 준비해주시길 바랍니다";
            }

            if (nextSchedule.Start.Date == now.Date)
            {
                return $"다음일정은 {nextSchedule.Item.Title}입니다";
            }

            return $"다음일정은 {nextSchedule.Start.Month}월 {nextSchedule.Start.Day}일 {FormatTime(nextSchedule.Start.TimeOfDay)} {nextSchedule.Item.Title}입니다";
        }

        public string FormatTime(string timeText)
        {
            return FormatTime(ParseTime(timeText));
        }

        public string FormatTime(TimeSpan time)
        {
            if (time == TimeSpan.FromHours(24))
            {
                return "24:00";
            }

            DateTime dateTime = DateTime.Today.Add(time);
            string shortTimePattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
            bool is12HourFormat = shortTimePattern.Contains("tt", StringComparison.OrdinalIgnoreCase);

            if (is12HourFormat)
            {
                return dateTime.ToString("tt h:mm", CultureInfo.CurrentCulture);
            }

            return dateTime.ToString("HH:mm", CultureInfo.CurrentCulture);
        }

        public string FormatRange(ScheduleItem item)
        {
            return $"{FormatTime(item.StartTime)} ~ {FormatTime(item.EndTime)}";
        }

        private ResolvedScheduleItem? GetCurrentOccurrence(DateTime now)
        {
            return GetResolvedSchedules()
                .FirstOrDefault(item => now >= item.Start && now < item.End);
        }

        private ResolvedScheduleItem? GetNextOccurrence(DateTime now)
        {
            return GetResolvedSchedules()
                .FirstOrDefault(item => item.Start > now);
        }

        private bool TryGetFirstScheduleStartDateTime(out DateTime result)
        {
            ResolvedScheduleItem? first = GetResolvedSchedules()
                .OrderBy(item => item.Start)
                .FirstOrDefault();

            if (first is null)
            {
                result = default;
                return false;
            }

            result = first.Start;
            return true;
        }

        private bool TryGetLastScheduleEndDateTime(out DateTime result)
        {
            ResolvedScheduleItem? last = GetResolvedSchedules()
                .OrderBy(item => item.End)
                .LastOrDefault();

            if (last is null)
            {
                result = default;
                return false;
            }

            result = last.End;
            return true;
        }

        private List<ResolvedScheduleItem> GetResolvedSchedules()
        {
            return _schedules
                .Select(CreateResolvedScheduleItem)
                .Where(item => item is not null)
                .Select(item => item!)
                .OrderBy(item => item.Start)
                .ToList();
        }

        private static ResolvedScheduleItem? CreateResolvedScheduleItem(ScheduleItem item)
        {
            if (!TryParseScheduleDate(item.Date, out DateTime day))
            {
                return null;
            }

            TimeSpan startTime = ParseTime(item.StartTime);
            TimeSpan endTime = ParseTime(item.EndTime);

            DateTime start = day.Add(startTime);
            DateTime end = day.Add(endTime);

            return new ResolvedScheduleItem(item, start, end);
        }

        private static bool TryParseScheduleDate(string dateText, out DateTime result)
        {
            result = default;

            if (string.IsNullOrWhiteSpace(dateText))
            {
                return false;
            }

            if (DateTime.TryParseExact(
                    dateText,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime fullDate))
            {
                result = fullDate.Date;
                return true;
            }

            if (DateTime.TryParseExact(
                    dateText,
                    "MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime monthDay))
            {
                try
                {
                    result = new DateTime(ScheduleYear, monthDay.Month, monthDay.Day);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        private static string FormatCountdown(TimeSpan remaining)
        {
            if (remaining <= TimeSpan.Zero)
            {
                return "00:00";
            }

            int totalSeconds = (int)Math.Ceiling(remaining.TotalSeconds);
            TimeSpan rounded = TimeSpan.FromSeconds(totalSeconds);

            if (rounded.TotalHours >= 1)
            {
                return rounded.ToString(@"hh\:mm\:ss");
            }

            return rounded.ToString(@"mm\:ss");
        }

        private static string FormatDayHourMinuteCountdown(TimeSpan remaining)
        {
            if (remaining <= TimeSpan.Zero)
            {
                return "0일 0시 0분";
            }

            int totalMinutes = (int)Math.Ceiling(remaining.TotalMinutes);

            int days = totalMinutes / (24 * 60);
            int hours = (totalMinutes % (24 * 60)) / 60;
            int minutes = totalMinutes % 60;

            return $"{days}일 {hours}시 {minutes}분";
        }

        private List<ScheduleItem> LoadSchedules()
        {
            string path = ResolveScheduleJsonPath();

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return new List<ScheduleItem>();
            }

            try
            {
                string json = File.ReadAllText(path);
                ScheduleRoot? root = JsonSerializer.Deserialize<ScheduleRoot>(json, JsonOptionsProvider.Default);

                if (root?.OverallSchedule is null)
                {
                    return new List<ScheduleItem>();
                }

                return root.OverallSchedule.ToList();
            }
            catch
            {
                return new List<ScheduleItem>();
            }
        }

        private static string ResolveScheduleJsonPath()
        {
            return DataFilePaths.GetMajorSchedulePath();
        }

        private static TimeSpan ParseTime(string timeText)
        {
            if (string.IsNullOrWhiteSpace(timeText))
            {
                return TimeSpan.Zero;
            }

            string[] parts = timeText.Split(':');

            if (parts.Length != 2)
            {
                return TimeSpan.Zero;
            }

            if (!int.TryParse(parts[0], out int hour))
            {
                return TimeSpan.Zero;
            }

            if (!int.TryParse(parts[1], out int minute))
            {
                return TimeSpan.Zero;
            }

            return new TimeSpan(hour, minute, 0);
        }

        private sealed class ResolvedScheduleItem
        {
            public ResolvedScheduleItem(ScheduleItem item, DateTime start, DateTime end)
            {
                Item = item;
                Start = start;
                End = end;
            }

            public ScheduleItem Item { get; }
            public DateTime Start { get; }
            public DateTime End { get; }
        }
    }
}