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
