using CopperBend.Contract;
using CopperBend.Fabric;
using Microsoft.Xna.Framework;
using System;

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
            : base()
        {
        }

        public Seed(Point point, int quantity, PlantType type)
            : base(point, quantity, true)
        {
            PlantType = type;
        }

        public override void ApplyTo(ITile tile, IControlPanel controls, IMessageOutput output, CmdDirection direction)
        {
            if (!tile.IsTilled)
            {
                string qualifier = tile.IsTillable ? "untilled " : "";
                output.WriteLine($"Cannot sow {qualifier}{tile.TileType.Name}.");
                return;
            }

            if (tile.IsSown)
            {
                output.WriteLine($"The ground to my {direction} is already sown with a seed.");
                return;
            }

            var seedToSow = GetSeedFromStack();
            tile.Sow(seedToSow);
            //controls.AddToSchedule(seedToSow, 100);

            if (--Quantity == 0)
            {
                //0.0
                //controls.RemoveFromInventory(this);
            }

            //controls.SetMapDirty();
            controls.AddExperience(seedToSow.PlantType, Exp.PlantSeed);
        }

        internal Seed GetSeedFromStack()
        {
            Guard.Against(Quantity < 1, "Somehow there's no seed here");
            return new Seed(this.Point, 1, this.PlantType);
        }


        private int growthRound = 0;

        public virtual Action<IControlPanel> GetNextAction()
        {
            throw new NotImplementedException();
        }

        private void SeedGrows(IControlPanel controls, IMessageOutput output)
        {
            output.WriteLine($"The seed is growing... Round {growthRound++}");
            controls.ScheduleAgent(this, growthRound > 2 ? 10 : 100);
        }

        protected virtual void SeedMatures(IControlPanel controls)
        {
            throw new Exception("Override or come up with a default implementation");
        }

        public ScheduleEntry GetNextEntry()
        {
            throw new NotImplementedException();
        }

        public ScheduleEntry GetNextEntry(int offset)
        {
            throw new NotImplementedException();
        }
    }

    public class HealerSeed : Seed
    {
        public HealerSeed(Point point, int quantity) 
            : base(point, quantity, PlantType.Healer)
        {}

        protected override void SeedMatures(IControlPanel controls)
        {
            //for now, insta-auto-harvest.  Two fruit drop to the ground, plant disappears.
            IItem fruit = new Fruit(this.Point, 2, this.PlantType);
            controls.PutItemOnMap(fruit);
            controls.RemovePlantAt(this.Point);
            //controls.SetMapDirty();
        }
    }
}
