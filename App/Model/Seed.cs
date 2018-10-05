namespace CopperBend.App.Model
{
    public class Seed : Item, ISeed
    {
        public SeedType SeedType;

        public override string Name { get => "seed"; }

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

        public override void ApplyTo(ITile tile, IAreaMap map, IControlPanel controls)
        {
            if (!tile.IsTilled)
            {
                string qualifier = tile.IsTillable ? "untilled " : "";
                controls.WriteLine($"Cannot sow {qualifier}{tile.TerrainType}.");
                return;
            }

            if (tile.IsSown)
            {
                controls.WriteLine("Already sown.");
                return;
            }

            if (--Quantity == 0)
            {
                //remove from inventory
            }

            tile.Sow(this);
            map.DisplayDirty = true;
            controls.PlayerBusyFor(15);
        }
    }
}
