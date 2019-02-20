using CopperBend.MapUtil;
using RLNET;

namespace CopperBend.App.Model
{
    public class Item : IItem, IDrawable
    {
        //  IDrawable
        public Point Point { get; protected set; }
        public RLColor ColorForeground { get; set; }
        public char Symbol { get; set; }
        public string Adjective { get; set; } = string.Empty;

        public void MoveTo(int x, int y)
        {
            Point = new Point(x, y);
        }

        public virtual string Name { get; set; }

        public Item(Point point)
        {
            Point = point;
            Quantity = 1;
        }

        public Item(Point point, int quantity, bool isUsable)
        {
            Point = point;
            Quantity = quantity;
            IsUsable = isUsable;
        }

        public void MoveTo(Point point)
        {
            Point = point;
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

        public virtual void ApplyTo(ITile tile, IControlPanel controls, IMessageOutput output, Direction direction)
        {
            //output.WriteLine($"Can't use a {Name} on {tile.TileType} to my {direction}.");
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
