using System.Collections.Generic;
using System.Linq;

namespace CopperBend.App
{
    public class Scheduler
    {
        private readonly SortedDictionary<int, List<ScheduleEntry>> _schedule;

        public int CurrentTick { get; private set; } = 0;

        public Scheduler()
        {
            _schedule = new SortedDictionary<int, List<ScheduleEntry>>();
        }

        //  Removes and returns the next thing to happen
        //  Ordered by tick of occurrence, then FIFO per tick
        public ScheduleEntry GetNext()
        {
            if (_schedule.Count() == 0) return null;

            var tickAgenda = _schedule.First();
            while (tickAgenda.Value.Count() == 0)
            {
                _schedule.Remove(tickAgenda.Key);
                tickAgenda = _schedule.First();
            }
            CurrentTick = tickAgenda.Key;

            var nextEntry = tickAgenda.Value.FirstOrDefault();
            if (nextEntry != null) Remove(nextEntry);
            return nextEntry;
        }

        public void DoNext(IControlPanel controls)
        {
            var nextUp = GetNext();
            nextUp.Action(controls, nextUp);
        }

        //  Entry scheduled at CurrentTick plus .TicksUntilNextAction
        public void Add(ScheduleEntry toAct)
        {
            if (toAct == null) return;

            int actionTick = CurrentTick + toAct.TicksUntilNextAction;
            if (!_schedule.ContainsKey(actionTick))
            {
                _schedule.Add(actionTick, new List<ScheduleEntry>());
            }
            _schedule[actionTick].Add(toAct);
        }

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

        public void RemoveActor(IActor targetActor)
        {
            foreach (var busyTick in _schedule)
            {
                busyTick.Value.RemoveAll(e => e.Actor == targetActor);
            }
        }
    }
}
