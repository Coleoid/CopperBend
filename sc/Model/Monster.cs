using System;
using Color = Microsoft.Xna.Framework.Color;

namespace CopperBend.Model
{
    public class Monster : Being
    {
        public override string BeingType { get; set; } = "Monster";

        public Monster(Guid blocker, Color foreground, Color background, int glyph = 'M', uint id = uint.MaxValue)
            : base(blocker, foreground, background, glyph, id)
        {
        }
    }
}
