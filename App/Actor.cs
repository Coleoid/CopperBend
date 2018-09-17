using RLNET;

namespace CopperBend.App
{
    public interface IActor
    {
        string Name { get; set; }
        int Awareness { get; set; }
    }

    public interface IDrawable
    {
        RLColor Color { get; set; }
        char Symbol { get; set; }
        int X { get; set; }
        int Y { get; set; }
    }

    public class Actor : IActor, IDrawable
    {
        // IActor
        public string Name { get; set; }
        public int Awareness { get; set; }

        // IDrawable
        public RLColor Color { get; set; }
        public char Symbol { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Player : Actor
    {
        public Player()
        {
            Name = "Our Dude";
            Symbol = '@';
            Color = Palette.DbLight;
            X = 1;
            Y = 1;
            Awareness = 4;
        }
    }
}
