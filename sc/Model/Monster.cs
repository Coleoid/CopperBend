using Color = Microsoft.Xna.Framework.Color;

namespace CopperBend.Model
{
    public class Monster : Being
    {
        public Monster(Color foreground, Color background, int glyph = 'M') 
            : base(foreground, background, glyph)
        {
        }
    }
}
