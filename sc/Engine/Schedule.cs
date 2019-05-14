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
        public int CurrentTick { get; private set; } = 0;
        private readonly SortedDictionary<int, List<Action<IControlPanel>>> TickEntries;
        private readonly ILog log;

        public Schedule()
        {
            TickEntries = new SortedDictionary<int, List<Action<IControlPanel>>>();
            log = LogManager.GetLogger("CB", "CB.Schedule");
        }

        //  Removes and returns the next to act
        //  Ordered by tick of occurrence, then FIFO per tick
        public Action<IControlPanel> GetNextAction()
        {
            log.DebugFormat("GetNextAction, tick {0}", CurrentTick);
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

        //  Action scheduled at CurrentTick plus offset
        public void AddEntry(ScheduleEntry entry)
        {
            Guard.AgainstNullArgument(entry);
            Guard.AgainstNullArgument(entry.Action);

            int actionTick = CurrentTick + entry.Offset;
            if (!TickEntries.ContainsKey(actionTick))
            {
                TickEntries.Add(actionTick, new List<Action<IControlPanel>>());
            }
            TickEntries[actionTick].Add(entry.Action);
        }

        public void Clear()
        {
            TickEntries.Clear();
        }

        public void AddAgent(IScheduleAgent agent)
        {
            AddEntry(agent.GetNextEntry());
        }

        public void AddAgent(IScheduleAgent agent, int offset)
        {
            AddEntry(agent.GetNextEntry(offset));
        }
    }

}
