﻿using System.Collections.Generic;
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

        //  Removes and returns the entity scheduled to act next
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

        public void DoNext(IGameState state)
        {
            var nextUp = GetNext();
            //if (nextUp == null) Debugger.Break();

            //  An action can return a new event to be scheduled
            var possibleNewEvent = nextUp.Action(nextUp, state);
            Add(possibleNewEvent);
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

        internal void RemoveActor(IActor targetActor)
        {
            foreach (var busyTick in _schedule)
            {
                var entries = busyTick.Value.Where(e => e.Actor == targetActor).ToList();
                foreach (var entry in entries)
                    busyTick.Value.Remove(entry);
            }
        }
    }
}
