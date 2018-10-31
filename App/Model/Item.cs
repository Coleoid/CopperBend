using RLNET;
using RogueSharp;

namespace CopperBend.App.Model
{
    public class Item : IItem, IDrawable
    {
        //  IDrawable
        public Coord Coord { get; protected set; }
        public RLColor ColorForeground { get; set; }
        public char Symbol { get; set; }
        public string Adjective { get; set; } = string.Empty;

        public void MoveTo(int x, int y)
        {
            Coord = new Coord(x, y);
        }

        public virtual string Name { get; set; }

        public Item(Coord coord)
        {
            Coord = coord;
            Quantity = 1;
        }

        public Item(Coord coord, int quantity, bool isUsable)
        {
            Coord = coord;
            Quantity = quantity;
            IsUsable = isUsable;
        }

        public void MoveTo(Coord coord)
        {
            Coord = coord;
        }

        public int Quantity { get; set; }
        public bool IsUsable { get; set; }

        public virtual bool IsConsumable => false;

        public virtual string ConsumeVerb => "eat";

        public virtual bool SameThingAs(IItem item)
        {
            return Name == item.Name
                && GetType() == item.GetType();
        }

        public virtual void ApplyTo(ITile tile, IControlPanel controls, Direction direction)
        {
            controls.WriteLine($"Can't use a {Name} on {tile.TileType} to my {direction}.");
        }

        public virtual void Consume(IControlPanel controls)
        {
            if (--Quantity < 1)
            {
                controls.RemoveFromInventory(this);
            }
        }
    }
}
