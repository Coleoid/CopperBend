using System;
using CopperBend.App.Basis;
using RogueSharp;

namespace CopperBend.App.Model
{
    public class Seed : Item, ISeed
    {
        public SeedType SeedType;

        public override string Name
        {
            get => "seed";
        }

        public override bool SameThingAs(IItem item)
        {
            if (item is Seed seed)
            {
                return SeedType == seed.SeedType;
            }

            return false;
        }

        public Seed(int x, int y, int quantity, SeedType type)
            : base(x, y, quantity, true)
        {
            SeedType = type;
        }

        public override void ApplyTo(ITile tile, IControlPanel controls)
        {
            if (!tile.IsTilled)
            {
                string qualifier = tile.IsTillable ? "untilled " : "";
                controls.WriteLine($"Cannot sow {qualifier}{tile.TerrainType}.");
                return;
            }

            if (tile.IsSown)
            {
                controls.WriteLine("Already sown with a seed.");
                return;
            }

            var sownSeed = new Seed(tile.X, tile.Y, 1, this.SeedType);
            tile.Sow(sownSeed);

            if (--Quantity == 0)
            {
                controls.RemoveFromInventory(this);
            }

            controls.AddToSchedule(new ScheduleEntry(100, SeedGrows));
            controls.SetMapDirty();
            controls.PlayerBusyFor(15);
        }

        private int growthRound = 0;

        private ScheduleEntry SeedGrows(ScheduleEntry entry, IControlPanel controls)
        {
            controls.WriteLine($"The seed is growing... Round {growthRound++}");
            if (growthRound > 9)
                return new ScheduleEntry(10, SeedMatures);
            else
                return new ScheduleEntry(100, SeedGrows);
        }

        protected virtual ScheduleEntry SeedMatures(ScheduleEntry entry, IControlPanel controls)
        {
            throw new Exception("Override or come up with a default implementation");
        }
    }

    public class HealerSeed : Seed
    {
        public HealerSeed(int x, int y, int quantity, SeedType type) 
            : base(x, y, quantity, type)
        {}

        protected override ScheduleEntry SeedMatures(ScheduleEntry entry, IControlPanel controls)
        {
            //for now, insta-auto-harvest
            IItem fruit = new Item(this.X, this.Y, 1, true);
            controls.PutItemOnMap(fruit);
            controls.RemovePlantAt(this);
            return null;
        }
    }
}
