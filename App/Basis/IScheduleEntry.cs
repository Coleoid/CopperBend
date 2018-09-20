using System;

namespace CopperBend.App
{
    public interface IScheduleEntry
    {
        int TicksUntilNextAction { get; }
        Func<IScheduleEntry> Action { get; }
    }
}
