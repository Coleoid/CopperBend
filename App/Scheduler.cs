using System;
using System.Collections.Generic;
using System.Linq;

namespace CopperBend.App
{
    public class Scheduler
    {
        private readonly SortedDictionary<int, List<ScheduleEntry>> _schedule;

        public int CurrentTick
        {
            get
            {
                if (_schedule.Count() == 0) return 0;
                return _schedule.First().Key;
            }
        }

        public Scheduler()
        {
            _schedule = new SortedDictionary<int, List<ScheduleEntry>>();
        }

        // New entry goes in at the current time plus .TicksUntilNextAction
        public void Add(ScheduleEntry toAct)
        {
            int actionTick = CurrentTick + toAct.TicksUntilNextAction;
            if (!_schedule.ContainsKey(actionTick))
            {
                _schedule.Add(actionTick, new List<ScheduleEntry>());
            }
            _schedule[actionTick].Add(toAct);
        }

        //  Returns the entity scheduled to act next, removed from the schedule
        public ScheduleEntry GetNext()
        {
            if (_schedule.Count() == 0) return null;

            var tickAgenda = _schedule.First();
            while (tickAgenda.Value.Count() == 0)
            {
                _schedule.Remove(tickAgenda.Key);
                tickAgenda = _schedule.First();
            }

            var nextDoer = tickAgenda.Value.FirstOrDefault();
            if (nextDoer != null) Remove(nextDoer);
            return nextDoer;
        }

        // Remove a specific object from the schedule.
        public void Remove(ScheduleEntry scheduleEntry)
        {
            foreach (var busyTick in _schedule)
            {
                if (busyTick.Value.Contains(scheduleEntry))
                {
                    busyTick.Value.Remove(scheduleEntry);
                    if (!busyTick.Value.Any())
                    {
                        _schedule.Remove(busyTick.Key);
                    }

                    return;
                }
            }
        }

        internal void RemoveActor(IActor targetActor)
        {
            foreach (var busyTick in _schedule)
            {
                var entries = busyTick.Value.Where(e => e.Actor == targetActor).ToList();
                foreach (var entry in entries)
                    busyTick.Value.Remove(entry);
            }
        }

        //// Clear out the schedule
        //public void Clear()
        //{
        //    _schedule.Clear();
        //}
    }
}
