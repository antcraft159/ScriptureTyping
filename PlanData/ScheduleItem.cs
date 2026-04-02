namespace ScriptureTyping.PlanData
{
    public sealed class ScheduleItem
    {
        public string Date { get; set; } = string.Empty;
        public string DateLabel { get; set; } = string.Empty;
        public string DayLabel { get; set; } = string.Empty;
        public int DayIndex { get; set; }
        public string Weekday { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}