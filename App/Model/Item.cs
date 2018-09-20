using RLNET;
using RogueSharp;

namespace CopperBend.App.Model
{
    public class Item : IItem, IDrawable, ICoord
    {
        public string Name { get; set; }

        public Item(int x, int y)
        {
            X = x;
            Y = y;
        }

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
    }
}
