using System.Collections.Generic;
using System.Linq;

namespace CopperBend.App
{
    public class Scheduler
    {
        private readonly SortedDictionary<int, List<IScheduleEntry>> _schedule;

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
            _schedule = new SortedDictionary<int, List<IScheduleEntry>>();
        }

        // New entry goes in at the current time plus .TicksUntilNextAction
        public void Add(IScheduleEntry toAct)
        {
            int actionTick = CurrentTick + toAct.TicksUntilNextAction;
            if (!_schedule.ContainsKey(actionTick))
            {
                _schedule.Add(actionTick, new List<IScheduleEntry>());
            }
            _schedule[actionTick].Add(toAct);
        }

        //  Returns the entity scheduled to act next, removed from the schedule
        public IScheduleEntry GetNext()
        {
            if (_schedule.Count() == 0) return null;

            var tickAgenda = _schedule.First();

            var nextDoer = tickAgenda.Value.First();
            Remove(nextDoer);
            return nextDoer;
        }

        // Remove a specific object from the schedule.
        public void Remove(IScheduleEntry scheduleEntry)
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

        //// Clear out the schedule
        //public void Clear()
        //{
        //    _schedule.Clear();
        //}
    }
}
