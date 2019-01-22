using System;
using System.Collections.Generic;

namespace CopperBend.App
{
    public class ScheduleEntry
    {
        public int Offset { get; }
        private Action<IControlPanel, ScheduleEntry> Action { get; }
        public IActor Actor { get; }
        public IEnumerable<IActor> Targets { get; }

        public ScheduleEntry(int offset, Action<IControlPanel, ScheduleEntry> action, IActor actor = null, IEnumerable<IActor> targets = null)
        {
            Guard.AgainstNullArgument(action, "A schedule entry needs an action.");
            Offset = offset;
            Action = action;
            Actor = actor;
            Targets = targets;
        }

        public void Execute(IControlPanel controls)
        {
            Action(controls, this);
        }
    }
}
