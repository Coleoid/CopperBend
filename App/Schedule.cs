using System;
using System.Collections.Generic;
using System.Linq;

namespace CopperBend.App
{
    public class Schedule : ISchedule
    {
        private readonly SortedDictionary<int, List<ScheduleEntry>> _entries;

        public int CurrentTick { get; private set; } = 0;

        public Schedule()
        {
            _entries = new SortedDictionary<int, List<ScheduleEntry>>();
        }

        //  Removes and returns the next thing to happen
        //  Ordered by tick of occurrence, then FIFO per tick
        public ScheduleEntry GetNext()
        {
            if (_entries.Count() == 0) return null;

            var tickAgenda = _entries.First();
            while (tickAgenda.Value.Count() == 0)
            {
                _entries.Remove(tickAgenda.Key);
                tickAgenda = _entries.First();
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

        //  Entry scheduled at CurrentTick plus .Offset
        public void Add(ScheduleEntry toAct)
        {
            if (toAct == null) return;

            int actionTick = CurrentTick + toAct.Offset;
            if (!_entries.ContainsKey(actionTick))
            {
                _entries.Add(actionTick, new List<ScheduleEntry>());
            }
            _entries[actionTick].Add(toAct);
        }

        public void Remove(ScheduleEntry scheduleEntry)
        {
            foreach (var busyTick in _entries)
            {
                if (busyTick.Value.Contains(scheduleEntry))
                {
                    busyTick.Value.Remove(scheduleEntry);
                    if (!busyTick.Value.Any())
                    {
                        _entries.Remove(busyTick.Key);
                    }

                    return;
                }
            }
        }

        public void RemoveActor(IActor targetActor)
        {
            foreach (var busyTick in _entries)
            {
                busyTick.Value.RemoveAll(e => e.Actor == targetActor);
            }
        }

        public void Clear()
        {
            _entries.Clear();
        }
    }
}
