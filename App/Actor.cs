using RLNET;
using RogueSharp;

namespace CopperBend.App
{
    public interface IActor
    {
        string Name { get; set; }
        int Awareness { get; set; }
    }

    public interface IDrawable : ICoord
    {
        RLColor Color { get; }
        char Symbol { get; }

        //  ICoord
        int X { get; set; }
        int Y { get; set; }
    }

    public class Actor : IActor, IDrawable, ICoord
    {
        //  IActor
        public string Name { get; set; }
        public int Awareness { get; set; }

        //  IDrawable
        public RLColor Color { get; set; }
        public char Symbol { get; set; }

        //  ICoord
        public int X { get; set; }
        public int Y { get; set; }
    }
}
