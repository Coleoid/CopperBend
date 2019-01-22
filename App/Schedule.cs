using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CopperBend.App
{
    public class Schedule : ISchedule
    {
        public int CurrentTick { get; private set; } = 0;
        private readonly SortedDictionary<int, List<ScheduleEntry>> TickEntries;
        private readonly ILog log;

        public Schedule()
        {
            TickEntries = new SortedDictionary<int, List<ScheduleEntry>>();
            log = LogManager.GetLogger("CB.Schedule");
        }

        //  Removes and returns the next thing to happen
        //  Ordered by tick of occurrence, then FIFO per tick
        public ScheduleEntry GetNext()
        {
            log.DebugFormat("GetNext at CurrentTick {0}", CurrentTick);
            if (TickEntries.Count() == 0) return null;

            var busyTick = TickEntries.First();
            while (busyTick.Value.Count() == 0)
            {
                TickEntries.Remove(busyTick.Key);
                busyTick = TickEntries.First();
            }
            CurrentTick = busyTick.Key;

            var nextEntry = busyTick.Value.First();
            Remove(nextEntry);
            return nextEntry;
        }

        public void DoNext(IControlPanel controls)
        {
            var entry = GetNext();
            entry.Execute(controls);
        }

        //  Entry scheduled at CurrentTick plus .Offset
        public void Add(ScheduleEntry entry)
        {
            //if (entry == null) return;
            Guard.AgainstNullArgument(entry);

            int actionTick = CurrentTick + entry.Offset;
            if (!TickEntries.ContainsKey(actionTick))
            {
                TickEntries.Add(actionTick, new List<ScheduleEntry>());
            }
            TickEntries[actionTick].Add(entry);
        }

        public void Remove(ScheduleEntry entry)
        {
            foreach (var busyTick in TickEntries)
            {
                if (busyTick.Value.Contains(entry))
                {
                    busyTick.Value.Remove(entry);
                    return;
                }
            }
            log.Debug("Remove did not find entry.");
        }

        public void RemoveActor(IActor targetActor)
        {
            foreach (var busyTick in TickEntries)
            {
                busyTick.Value.RemoveAll(e => e.Actor == targetActor);
            }
        }

        public void Clear()
        {
            TickEntries.Clear();
        }
    }
}
