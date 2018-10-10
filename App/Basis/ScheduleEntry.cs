using System;

namespace CopperBend.App
{
    public class ScheduleEntry
    {
        public int TicksUntilNextAction { get; private set; }

        public Func<ScheduleEntry, IControlPanel, ScheduleEntry> Action { get; private set; }

        public IActor Actor { get; private set; }

        public ScheduleEntry(
            int ticks, 
            Func<ScheduleEntry, IControlPanel, ScheduleEntry> action)
        {
            TicksUntilNextAction = ticks;
            Action = action;
            Actor = null;
        }

        public ScheduleEntry(int ticks, IActor actor)
        {
            TicksUntilNextAction = ticks;
            Action = actor.NextAction;
            Actor = actor;
        }
    }
}
