using System;

namespace CopperBend.App
{
    public class ScheduleEntry
    {
        public int Offset { get; private set; }

        public Action<IControlPanel, ScheduleEntry> Action { get; private set; }

        public IActor Actor { get; private set; }

        public ScheduleEntry(int offset, Action<IControlPanel, ScheduleEntry> action)
        {
            Guard.Against(action == null, "A schedule entry needs an action or an actor.");
            Offset = offset;
            Action = action;
        }

        public ScheduleEntry(int offset, IActor actor)
        {
            Guard.Against(actor == null, "A schedule entry needs an action or an actor.");
            Offset = offset;
            Action = actor.NextAction;
            Actor = actor;
        }
    }
}
