using System.Collections.Generic;

namespace ScriptureTyping.PlanData
{
    public sealed class ScheduleRoot
    {
        public List<ScheduleItem> OverallSchedule { get; set; } = new();
    }
}