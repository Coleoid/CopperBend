using RLNET;
using RogueSharp;

namespace CopperBend.App
{
    public class Item : IDrawable, ICoord
    {
        public string Name { get; set; }

        //  IDrawable
        public RLColor Color { get; set; }
        public char Symbol { get; set; }

        //  ICoord
        public int X { get; set; }
        public int Y { get; set; }
    }
}
