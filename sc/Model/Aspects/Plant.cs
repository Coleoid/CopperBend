using System.Collections.Generic;
using CopperBend.Contract;

namespace CopperBend.Model
{
    public class Plant : IScheduleAgent
    {
        public PlantDetails PlantDetails { get; set; }

        public Plant(PlantDetails details, ISchedule schedule = null)
        {
            PlantDetails = details;
            schedule?.AddAgent(this);
            // for IScheduleAgent aspects, do I need to link back to my entity?
        }

        public ScheduleEntry GetNextEntry(int offset)
        {
            //0.0: plant needs to grow, then when grown, act depending on its type
            //currently, remove itself from the schedule
            return new ScheduleEntry { Action = ScheduleAction.NoFurtherActions, Agent = this, Offset = offset };
        }
    }

    public class GrowingPlant : IScheduleAgent
    {
        public PlantDetails PlantDetails { get; set; }
        public GrowingPlant(PlantDetails details, ISchedule schedule)
        {
            PlantDetails = details;
            schedule.AddAgent(this);
        }

        public ScheduleEntry GetNextEntry(int offset)
        {
            return new ScheduleEntry
            {
                Action = ScheduleAction.PlantGrows,
                Offset = offset,
            };
        }
    }

    public class PlantDetails
    {
        public uint ID { get; set; }
        public string MainName { get; set; }
        public int GrowthTime { get; set; }
        public bool SeedKnown { get; set; }
        public bool FruitKnown { get; set; }
        public string SeedAdjective { get; set; }
        public string FruitAdjective { get; set; }
        public string SeedDescriptionAsKnown => $"{(SeedKnown ? MainName : SeedAdjective)} seed";
        public string FruitDescriptionAsKnown => FruitKnown ? MainName : $"{FruitAdjective} fruit";
        public List<(PlantPart, PlantUse, string)> Uses { get; }  //0.+
    }
}
