using System;

namespace CopperBend.App
{
    public class ScheduleEntry
    {
        public int TicksUntilNextAction { get; private set; }

        public Action<IControlPanel, ScheduleEntry> Action { get; private set; }

        public IActor Actor { get; private set; }

        public ScheduleEntry(int ticks, Action<IControlPanel, ScheduleEntry> action, IActor actor = null)
        {
            Guard.Against(action == null && actor == null, "A schedule entry needs an action or an actor.");
            TicksUntilNextAction = ticks;
            Action = action ?? actor.NextAction;
            Actor = actor;
        }
    }
}
