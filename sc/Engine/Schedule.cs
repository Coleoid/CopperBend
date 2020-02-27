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
        private readonly SortedDictionary<int, List<ScheduleEntry>> tickEntries;
        public int CurrentTick { get; private set; }

        public Schedule(ILog logger)
        {
            log = logger;
            tickEntries = new SortedDictionary<int, List<ScheduleEntry>>();
            CurrentTick = 0;
        }

        /// <summary> Get next scheduled action, ordered by tick of occurrence, then FIFO per tick </summary>
        public ScheduleEntry GetNextAction()
        {
            log.DebugFormat("Schedule.GetNextAction @ tick {0}", CurrentTick);
            if (tickEntries.Count == 0)
                throw new Exception("The Schedule should never empty out");

            var busyTick = tickEntries.First();
            while (busyTick.Value.Count == 0)
            {
                tickEntries.Remove(busyTick.Key);
                //log.Debug($"Removed empty tick {busyTick.Key} from the schedule");
                if (tickEntries.Count == 0)
                    throw new Exception("The Schedule should never empty out");
                busyTick = tickEntries.First();
            }
            CurrentTick = busyTick.Key;

            var nextAction = busyTick.Value.First();
            busyTick.Value.RemoveAt(0);
            return nextAction;
        }

        public void AddAgent(IScheduleAgent agent)
        {
            AddAgent(agent, 12); //0.1
        }

        public void AddAgent(IScheduleAgent agent, int offset)
        {
            var entry = agent.GetNextEntry(offset);
            AddEntry(entry);
        }

        //  Action scheduled at CurrentTick plus offset
        public void AddEntry(ScheduleEntry entry)
        {
            Guard.AgainstNullArgument(entry);
            Guard.Against(entry.Action == ScheduleAction.Unset);

            int actionTick = CurrentTick + entry.Offset;
            if (!tickEntries.ContainsKey(actionTick))
            {
                tickEntries.Add(actionTick, new List<ScheduleEntry>());
            }
            tickEntries[actionTick].Add(entry);
        }

        public void RemoveAgent(IScheduleAgent agent)
        {
            foreach (var key in tickEntries.Keys)
            {
                var entries = tickEntries[key];
                entries.RemoveAll(e => e.Agent == agent);
            }
        }

        public void Clear()
        {
            tickEntries.Clear();
            CurrentTick = 0;
        }
    }
}
