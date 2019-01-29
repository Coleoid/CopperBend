using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CopperBend.App
{
    public class Schedule : ISchedule
    {
        public int CurrentTick { get; private set; } = 0;
        private readonly SortedDictionary<int, List<Action<IControlPanel>>> TickEntries;
        private readonly ILog log;

        public Schedule()
        {
            TickEntries = new SortedDictionary<int, List<Action<IControlPanel>>>();
            log = LogManager.GetLogger("CB.Schedule");
        }

        //  Removes and returns the next to act
        //  Ordered by tick of occurrence, then FIFO per tick
        public Action<IControlPanel> GetNextAction()
        {
            log.DebugFormat("GetNext at CurrentTick {0}", CurrentTick);
            if (TickEntries.Count() == 0) return null;

            var busyTick = TickEntries.First();
            while (busyTick.Value.Count() == 0)
            {
                TickEntries.Remove(busyTick.Key);
                if (TickEntries.Count() == 0) return null;
                busyTick = TickEntries.First();
            }
            CurrentTick = busyTick.Key;

            var nextAction = busyTick.Value.First();
            busyTick.Value.RemoveAt(0);
            return nextAction;
        }

        public void DoNext(IControlPanel controls)
        {
            var nextAction = GetNextAction();
            nextAction?.Invoke(controls);
        }

        //  Action scheduled at CurrentTick plus offset
        public void Add(Action<IControlPanel> action, int offset)
        {
            Guard.AgainstNullArgument(action);

            int actionTick = CurrentTick + offset;
            if (!TickEntries.ContainsKey(actionTick))
            {
                TickEntries.Add(actionTick, new List<Action<IControlPanel>>());
            }
            TickEntries[actionTick].Add(action);
        }

        public void Clear()
        {
            TickEntries.Clear();
        }
    }
}
