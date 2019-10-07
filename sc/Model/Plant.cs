using System;
using CopperBend.Contract;

namespace CopperBend.Model
{
    public class Plant : IScheduleAgent
    {
        public ScheduleEntry GetNextEntry(int offset)
        {
            throw new NotImplementedException();
        }
    }

    public class GrowingPlant : IScheduleAgent
    {
        public PlantDetails PlantDetails { get; protected set; }
        public GrowingPlant(Seed seed, ISchedule schedule)
        {
            PlantDetails = seed.PlantDetails;
            schedule.AddAgent(this);
        }

        public ScheduleEntry GetNextEntry(int offset)
        {
            return new ScheduleEntry
            {
                Action = ScheduleAction.PlantGrows,
                Offset = offset
            };
        }
    }

}
