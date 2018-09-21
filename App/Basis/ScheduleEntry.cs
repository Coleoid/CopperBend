using System;

namespace CopperBend.App
{
    public class ScheduleEntry
    {
        public int TicksUntilNextAction { get; private set; }

        public Func<ScheduleEntry, IAreaMap, IActor, ScheduleEntry> Action { get; private set; }

        public IActor Actor { get; private set; }

        public ScheduleEntry(
            int ticks, 
            Func<ScheduleEntry, IAreaMap, IActor, ScheduleEntry> action)
        {
            TicksUntilNextAction = ticks;
            Action = action;
            Actor = null;
        }

        public ScheduleEntry(int ticks, IActor actor)
        {
            TicksUntilNextAction = ticks;
            Action = actor.Strategy;
            Actor = actor;
        }
    }
}
