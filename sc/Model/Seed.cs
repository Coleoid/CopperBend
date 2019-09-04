using System.Collections.Generic;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Fabric;

namespace CopperBend.Model
{
    public class Seed : Item, ISeed, IScheduleAgent
    {
        public override string ItemType { get => "Seed"; }
        public static Dictionary<uint, PlantDetails> PlantByID { get; set; }
        public static Dictionary<string, PlantDetails> PlantByName { get; set; }

        public PlantDetails PlantDetails;

        public override bool StacksWith(IItem item)
        {
            if (item is Seed seed)
            {
                return PlantDetails == seed.PlantDetails;
            }

            return false;
        }

        public Seed()
            : base((0,0), 1, true)
        { }

        public Seed(Coord position, int quantity, uint typeID, uint id = uint.MaxValue)
            : base(position, quantity, true, id)
        {
            PlantDetails = PlantByID[typeID];
        }

        public Seed(Coord position, PlantDetails details, int quantity = 1, uint id = uint.MaxValue)
            : base(position, quantity, true, id)
        {
            PlantDetails = details;
        }

        internal Seed GetSeedFromStack()
        {
            Guard.Against(Quantity < 1, "Somehow there's no seed here");
            return new Seed(this.Location, 1, this.PlantDetails.ID);
        }

        public override string Name
        {
            get => PlantDetails.SeedDescriptionAsKnown;
        }

        private int growthRound = 0;

        public void SeedGrows(IControlPanel controls)
        {
            controls.WriteLine($"The seed is growing... Round {growthRound++}");
            if (growthRound > 3)
                SeedMatures(controls);
            else
                controls.ScheduleAgent(this, growthRound > 2 ? 10 : 40);
        }

        protected virtual void SeedMatures(IControlPanel controls)
        {
            //for now, insta-auto-harvest.  Two fruit drop to the ground, plant disappears.
            IItem fruit = new Fruit(this.Location, 2, this.PlantDetails);
            controls.PutItemOnMap(fruit);
            controls.RemovePlantAt(this.Location);
        }

        public ScheduleEntry GetNextEntry()
        {
            return GetNextEntry(100);
        }

        public ScheduleEntry GetNextEntry(int offset)
        {
            return new ScheduleEntry
            {
                Action = ScheduleAction.SeedGrows,
                Agent = this,
                Offset = offset
            };
        }
    }
}
