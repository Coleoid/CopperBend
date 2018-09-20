namespace CopperBend.App.Model
{
    public class Player : Actor
    {
        public Player()
            : base()
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
