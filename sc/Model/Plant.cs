using System;
using CopperBend.Contract;

namespace CopperBend.Model
{
    public class Plant : IScheduleAgent
    {
        public ScheduleEntry GetNextEntry()
        {
            throw new NotImplementedException();
        }

        public ScheduleEntry GetNextEntry(int offset)
        {
            throw new NotImplementedException();
        }
    }

    public class GrowingPlant : IScheduleAgent
    {
        public PlantType PlantType { get; protected set; }
        public GrowingPlant(Seed seed, ISchedule schedule)
        {
            PlantType = seed.PlantType;
            schedule.AddAgent(this);
        }

        public ScheduleEntry GetNextEntry()
        {
            return GetNextEntry(88); // this looks less and less relevant
        }

        public ScheduleEntry GetNextEntry(int offset)
        {
            return new ScheduleEntry
            {
                Action = (cp) => throw new Exception($"Make code to grow plant {PlantType}!"),
                Offset = offset
            };
        }
    }

}
