using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using CopperBend.Contract;
using CopperBend.Fabric;

namespace CopperBend.Engine
{
    public class Schedule : ISchedule
    {
        private readonly ILog log;
        private readonly SortedDictionary<int, List<ScheduleEntry>> TickEntries;
        public int CurrentTick { get; private set; }

        public Schedule()
        {
            log = LogManager.GetLogger("CB", "CB.Schedule");
            TickEntries = new SortedDictionary<int, List<ScheduleEntry>>();
            CurrentTick = 0;
        }

        /// <summary> Get next scheduled action, ordered by tick of occurrence, then FIFO per tick </summary>
        public ScheduleEntry GetNextAction()
        {
            log.DebugFormat("Schedule.GetNextAction @ tick {0}", CurrentTick);
            if (TickEntries.Count() == 0)
                throw new Exception("The Schedule should never empty out");

            var busyTick = TickEntries.First();
            while (busyTick.Value.Count() == 0)
            {
                TickEntries.Remove(busyTick.Key);
                //log.Debug($"Removed empty tick {busyTick.Key} from the schedule");
                if (TickEntries.Count() == 0)
                    throw new Exception("The Schedule should never empty out");
                busyTick = TickEntries.First();
            }
            CurrentTick = busyTick.Key;

            var nextAction = busyTick.Value.First();
            busyTick.Value.RemoveAt(0);
            return nextAction;
        }

        public void AddAgent(IScheduleAgent agent)
        {
            AddEntry(agent.GetNextEntry());
        }

        public void AddAgent(IScheduleAgent agent, int offset)
        {
            AddEntry(agent.GetNextEntry(offset));
        }

        //  Action scheduled at CurrentTick plus offset
        public void AddEntry(ScheduleEntry entry)
        {
            Guard.AgainstNullArgument(entry);
            Guard.Against(entry.Action == ScheduleAction.Unset);

            int actionTick = CurrentTick + entry.Offset;
            if (!TickEntries.ContainsKey(actionTick))
            {
                TickEntries.Add(actionTick, new List<ScheduleEntry>());
            }
            TickEntries[actionTick].Add(entry);
        }

        public void Clear()
        {
            TickEntries.Clear();
        }
    }
}
