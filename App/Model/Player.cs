namespace CopperBend.App.Model
{
    public class Player : Actor
    {
        public Player()
            : base(1, 1)
        {
            Name = "Our Dude";
            Symbol = '@';
            Color = Palette.DbLight;
            Awareness = 4;
        }
    }
}
