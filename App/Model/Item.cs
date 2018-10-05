using RLNET;
using RogueSharp;

namespace CopperBend.App.Model
{
    public class Item : IItem, IDrawable, ICoord
    {
        //  IDrawable
        public RLColor Color { get; set; }
        public char Symbol { get; set; }
        public void MoveTo(int x, int y)
        {
            X = x;
            Y = y;
        }

        //  ICoord
        public int X { get; private set; }
        public int Y { get; private set; }


        public virtual string Name { get; set; }

        public Item(int x, int y)
        {
            X = x;
            Y = y;
            Quantity = 1;
        }

        public Item(int x, int y, int quantity, bool isUsable)
        {
            X = x;
            Y = y;
            Quantity = quantity;
            IsUsable = isUsable;
        }

        public int Quantity { get; set; }
        public bool IsUsable { get; set; }

        public virtual bool SameThingAs(IItem item)
        {
            return Name == item.Name
                && GetType() == item.GetType();
        }

        public virtual void ApplyTo(ITile tile, IAreaMap map, IControlPanel controls)
        {
            controls.WriteLine($"Can't use a {Name} on {tile.TerrainType}.");
        }
    }

    public interface IControlPanel
    {
        void WriteLine(string text);
        void Prompt(string text);
        void PlayerBusyFor(int ticks);
    }
}
