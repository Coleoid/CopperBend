using System;

namespace CopperBend.App.Model
{
    public class ScheduleEntry : IScheduleEntry
    {
        public int TicksUntilNextAction { get; private set; }

        public Func<IScheduleEntry> Action { get; private set; }

        public ScheduleEntry(int ticks, Func<IScheduleEntry> action)
        {
            TicksUntilNextAction = ticks;
            Action = action;
        }
    }
}
