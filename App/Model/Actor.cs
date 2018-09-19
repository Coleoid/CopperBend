using RLNET;
using RogueSharp;

namespace CopperBend.App.Model
{
    public class Actor : IActor//, IDrawable, ICoord
    {
        //  IActor
        public string Name { get; set; }
        public int Awareness { get; set; }

        //  IDrawable
        public RLColor Color { get; set; }
        public char Symbol { get; set; }
        public void MoveTo(int x, int y)
        {
            X = x;
            Y = y;
        }

        //  ICoord
        public int X { get; protected set; }
        public int Y { get; protected set; }
    }
}
