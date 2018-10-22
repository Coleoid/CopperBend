using System;

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
                controls.WriteLine($"Cannot sow {qualifier}{tile.TileType}.");
                return;
            }

            if (tile.IsSown)
            {
                controls.WriteLine("Already sown with a seed.");
                return;
            }

            //PROBLEM:  Splitting stacks in a base class, creating a new subclass instance...
            var sownSeed = new HealerSeed(tile.X, tile.Y, 1);
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
        public HealerSeed(int x, int y, int quantity) 
            : base(x, y, quantity, SeedType.Healer)
        {}

        protected override void SeedMatures(IControlPanel controls, ScheduleEntry entry)
        {
            //for now, insta-auto-harvest.  Two fruit drop to the ground, plant disappears.
            IItem fruit = new Fruit(this.X, this.Y, 2, SeedType.Healer);
            controls.PutItemOnMap(fruit);
            controls.RemovePlantAt(this);
            controls.SetMapDirty();
        }
    }
}
