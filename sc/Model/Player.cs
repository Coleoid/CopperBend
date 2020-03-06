using System;
using Color = Microsoft.Xna.Framework.Color;

namespace CopperBend.Model
{
    public class Player : Being
    {
        public override string BeingType { get; set; } = "Player";

        public Player(Guid blocker, Color foreground, Color background, int glyph = '@', uint id = uint.MaxValue)
            : base(blocker, foreground, background, glyph, id)
        {
            IsPlayer = true;
        }
    }
}
