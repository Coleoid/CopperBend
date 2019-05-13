using Color = Microsoft.Xna.Framework.Color;
using GoRogue;
using CopperBend.Contract;

namespace CopperBend.Model
{
    public class Item : IItem
    {
        public Item(Coord location, int quantity, bool isUsable)
        {
            Location = location;
            Quantity = quantity;
            IsUsable = isUsable;
        }

        public Item(Coord location)
        {
            Location = location;
            Quantity = 1;
        }

        public Item()
        {
            Quantity = 1;
        }

        // one IDGenerator for all Items
        public static IDGenerator IDGenerator = new IDGenerator();
        public uint ID { get; private set; } = IDGenerator.UseID();

        public Color Foreground { get; set; }
        public int Glyph { get; set; }

        public virtual string Name { get; set; }
        public string Adjective { get; set; } = string.Empty;
        public int Quantity { get; set; }

        public Coord Location { get; protected set; }

        public void MoveTo(int x, int y)
        {
            Location = (x, y);
        }

        public void MoveTo(Coord location)
        {
            Location = location;
        }

        public bool IsUsable { get; set; }

        public virtual void ApplyTo(Coord position, IControlPanel controls, ILogWindow output, CmdDirection direction)
        {
            //output.Add($"Can't use a {Name} on {tile.TileType} to my {direction}.");
        }

        public virtual bool IsConsumable => false;

        public virtual string ConsumeVerb => "eat";

        public virtual bool SameThingAs(IItem item)
        {
            return Name == item.Name
                && GetType() == item.GetType();
        }
    }
}

