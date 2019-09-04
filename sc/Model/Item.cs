using Color = Microsoft.Xna.Framework.Color;
using GoRogue;
using CopperBend.Contract;

namespace CopperBend.Model
{
    public class Item : IItem
    {
        public string ItemType { get; }
        public Item(Coord location, int quantity = 1, bool isUsable = false, uint id = uint.MaxValue)
        {
            ID = (id == uint.MaxValue ? IDGenerator.UseID() : id);
            Location = location;
            Quantity = quantity;
            IsUsable = isUsable;
        }

        public static IDGenerator IDGenerator;
        public uint ID { get; private set; }

        public Color Foreground { get; set; }
        public int Glyph { get; set; }

        public virtual string Name { get; set; }
        public virtual string Adjective { get; set; } = string.Empty;
        public int Quantity { get; set; }

        public Coord Location { get; set; }

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

        public virtual bool StacksWith(IItem item)
        {
            return Name == item.Name
                && GetType() == item.GetType();
        }
    }
}
