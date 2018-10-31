﻿using RogueSharp;
using System;

namespace CopperBend.App.Model
{
    public class Seed : Item, ISeed
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

        public Seed(Coord coord, int quantity, PlantType type)
            : base(coord, quantity, true)
        {
            PlantType = type;
        }

        public override void ApplyTo(ITile tile, IControlPanel controls, Direction direction)
        {
            if (!tile.IsTilled)
            {
                string qualifier = tile.IsTillable ? "untilled " : "";
                controls.WriteLine($"Cannot sow {qualifier}{tile.TileType.Name}.");
                return;
            }

            if (tile.IsSown)
            {
                controls.WriteLine($"The ground to my {direction} is already sown with a seed.");
                return;
            }

            //PROBLEM:  Splitting stacks in a base class, creating a new subclass instance...
            var sownSeed = new HealerSeed(tile.Coord, 1);  // bug
            tile.Sow(sownSeed);

            if (--Quantity == 0)
            {
                controls.RemoveFromInventory(this);
            }

            controls.AddToSchedule(new ScheduleEntry(100, sownSeed.SeedGrows));
            controls.SetMapDirty();
            controls.PlayerBusyFor(15);
        }

        private int growthRound = 0;

        private void SeedGrows(IControlPanel controls, ScheduleEntry entry)
        {
            controls.WriteLine($"The seed is growing... Round {growthRound++}");
            controls.AddToSchedule( growthRound > 2 ? 
                  new ScheduleEntry(10, SeedMatures)
                : new ScheduleEntry(100, SeedGrows));
        }

        protected virtual void SeedMatures(IControlPanel controls, ScheduleEntry entry)
        {
            throw new Exception("Override or come up with a default implementation");
        }
    }

    public class HealerSeed : Seed
    {
        public HealerSeed(Coord coord, int quantity) 
            : base(coord, quantity, PlantType.Healer)
        {}

        protected override void SeedMatures(IControlPanel controls, ScheduleEntry entry)
        {
            //for now, insta-auto-harvest.  Two fruit drop to the ground, plant disappears.
            IItem fruit = new Fruit(this.Coord, 2, PlantType.Healer);
            controls.PutItemOnMap(fruit);
            controls.RemovePlantAt(this.Coord);
            controls.SetMapDirty();
        }
    }
}
