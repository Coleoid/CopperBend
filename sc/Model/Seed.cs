using System.Collections.Generic;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model.Aspects;

namespace CopperBend.Model
{
    public class Seed : Item, ISeed, IScheduleAgent
    {
        public override string ItemType { get => "Seed"; }
        public static Herbal Herbal { get; set; }

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

        public Seed(uint typeID, int quantity = 1, uint id = uint.MaxValue)
            : base((0,0), quantity, true, id)
        {
            PlantDetails = Herbal.PlantByID[typeID];
        }

        public Seed(PlantDetails details, int quantity = 1, uint id = uint.MaxValue)
            : base((0,0), quantity, true, id)
        {
            PlantDetails = details;
        }

        internal Seed GetSeedFromStack()
        {
            Guard.Against(Quantity < 1, "Somehow there's no seed here");
            Seed seed = new Seed(this.PlantDetails);
            Quantity--;
            seed.Location = Location;
            return seed;
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
            //0.1:  Move beyond insta-auto-harvest.
            //Two fruit drop to the ground, plant disappears.
            IItem fruit = new Item(this.Location, 2);
            fruit.Aspects.AddComponent(new Consumable {
                IsFruit = true,
                PlantID = this.PlantDetails.ID, 
            });
            controls.PutItemOnMap(fruit);
            controls.RemovePlantAt(this.Location);
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
