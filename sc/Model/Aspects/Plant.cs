using System;
using System.Collections.Generic;
using CopperBend.Contract;

namespace CopperBend.Model
{
    public class PlantDetails
    {
        public uint ID;
        public string MainName { get; set; }
        public int GrowthTime { get; set; }
        public bool SeedKnown;
        public bool FruitKnown;
        public string SeedAdjective;
        public string FruitAdjective;
        public string SeedDescriptionAsKnown => $"{(SeedKnown ? MainName : SeedAdjective)} seed";
        public string FruitDescriptionAsKnown => FruitKnown ? MainName : $"{FruitAdjective} fruit";
        public List<(PlantPart, PlantUse, string)> Uses;  //0.+
    }

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
