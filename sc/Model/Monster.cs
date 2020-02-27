using Color = Microsoft.Xna.Framework.Color;

namespace CopperBend.Model
{
    public class Monster : Being
    {
        public override string BeingType { get; set; } = "Monster";

        public Monster(Color foreground, Color background, int glyph = 'M', uint id = uint.MaxValue)
            : base(foreground, background, glyph, id)
        {
        }
    }
}
