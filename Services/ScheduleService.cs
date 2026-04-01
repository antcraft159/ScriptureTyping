using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using ScriptureTyping.PlanData;

namespace ScriptureTyping.Services
{
    public sealed class ScheduleService
    {
        private readonly List<ScheduleItem> _schedules;

        public ScheduleService()
        {
            _schedules = LoadSchedules();
        }

        public IReadOnlyList<ScheduleItem> GetTodaySchedules(DateTime now)
        {
            string todayKey = now.ToString("MM-dd", CultureInfo.InvariantCulture);

            return _schedules
                .Where(item => string.Equals(item.Date, todayKey, StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => ParseTime(item.StartTime))
                .ToList();
        }

        public ScheduleItem? GetCurrentSchedule(DateTime now)
        {
            TimeSpan currentTime = now.TimeOfDay;

            return GetTodaySchedules(now)
                .FirstOrDefault(item =>
                {
                    TimeSpan start = ParseTime(item.StartTime);
                    TimeSpan end = ParseTime(item.EndTime);
                    return currentTime >= start && currentTime < end;
                });
        }

        public ScheduleItem? GetNextSchedule(DateTime now)
        {
            TimeSpan currentTime = now.TimeOfDay;

            return GetTodaySchedules(now)
                .FirstOrDefault(item => ParseTime(item.StartTime) > currentTime);
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

        private List<ScheduleItem> LoadSchedules()
        {
            string path = ResolveScheduleJsonPath();

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return new List<ScheduleItem>();
            }

            string json = File.ReadAllText(path);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            ScheduleRoot? root = JsonSerializer.Deserialize<ScheduleRoot>(json, options);

            if (root?.OverallSchedule is null)
            {
                return new List<ScheduleItem>();
            }

            return root.OverallSchedule
                .OrderBy(item => item.Date)
                .ThenBy(item => ParseTime(item.StartTime))
                .ToList();
        }

        private static string ResolveScheduleJsonPath()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string[] candidates =
            {
                Path.Combine(baseDirectory, "PlanData", "major_schedule.json"),
                Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\..\PlanData\major_schedule.json")),
                Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\..\..\PlanData\major_schedule.json")),
                Path.Combine(Environment.CurrentDirectory, "PlanData", "major_schedule.json")
            };

            foreach (string candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return string.Empty;
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
    }
}