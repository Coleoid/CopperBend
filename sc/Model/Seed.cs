﻿using System;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Fabric;

namespace CopperBend.Model
{
    public class Seed : Item, ISeed, IScheduleAgent
    {
        public PlantType PlantType;

        public override string Name
        {
            get => "seed";
        }

        public override bool SameThingAs(IItem item)
        {
            if (item is Seed seed)
            {
                return PlantType == seed.PlantType;
            }

            return false;
        }

        public Seed()
            : base((0,0), 1, true)
        {
        }

        public Seed(Coord position, int quantity, PlantType type)
            : base(position, quantity, true)
        {
            PlantType = type;
        }

        //public override void ApplyTo(Coord position, IControlPanel controls, ILogWindow output, CmdDirection direction)
        //{
        //    //if (!tile.IsTilled)
        //    //{
        //    //    string qualifier = tile.IsTillable ? "untilled " : "";
        //    //    output.Add($"Cannot sow {qualifier}{tile.TileType.Name}.");
        //    //    return;
        //    //}

        //    //if (tile.IsSown)
        //    //{
        //    //    output.Add($"The ground to my {direction} is already sown with a seed.");
        //    //    return;
        //    //}

        //    //var seedToSow = GetSeedFromStack();
        //    //tile.Sow(seedToSow);
        //    ////controls.AddToSchedule(seedToSow, 100);

        //    if (--Quantity == 0)
        //    {
        //        //0.0
        //        //controls.RemoveFromInventory(this);
        //    }

        //    var seedToSow = GetSeedFromStack();
        //    controls.AddExperience(seedToSow.PlantType, Exp.PlantSeed);
        //}

        internal Seed GetSeedFromStack()
        {
            Guard.Against(Quantity < 1, "Somehow there's no seed here");
            return new Seed(this.Location, 1, this.PlantType);
        }


        private int growthRound = 0;

        public void SeedGrows(IControlPanel controls)
        {
            controls.AddMessage($"The seed is growing... Round {growthRound++}");
            if (growthRound > 3)
                SeedMatures(controls);
            else
                controls.ScheduleAgent(this, growthRound > 2 ? 10 : 40);
        }

        protected virtual void SeedMatures(IControlPanel controls)
        {
            throw new Exception("Outcome of seed maturity dependent on seed type.  Must override.");
        }

        public ScheduleEntry GetNextEntry()
        {
            return GetNextEntry(100);
        }

        public ScheduleEntry GetNextEntry(int offset)
        {
            return new ScheduleEntry
            {
                Action = (cp) => SeedGrows(cp),
                Agent = this,
                Offset = offset
            };
        }
    }

    public class HealerSeed : Seed
    {
        public HealerSeed(Coord position, int quantity) 
            : base(position, quantity, PlantType.Healer)
        {}

        protected override void SeedMatures(IControlPanel controls)
        {
            //for now, insta-auto-harvest.  Two fruit drop to the ground, plant disappears.
            IItem fruit = new Fruit(this.Location, 2, this.PlantType);
            controls.PutItemOnMap(fruit);
            controls.RemovePlantAt(this.Location);
        }
    }
}
