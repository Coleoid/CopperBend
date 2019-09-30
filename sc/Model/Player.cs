using Color = Microsoft.Xna.Framework.Color;
using SadConsole.Components;

namespace CopperBend.Model
{
    public class Player : Being
    {
        public override string BeingType { get; set; } = "Player";

        public Player(Color foreground, Color background, int glyph = '@', uint id = uint.MaxValue)
            : base(foreground, background, glyph, id)
        {
            IsPlayer = true;
        }

        internal void AddComponent(IConsoleComponent component)
        {
            SadConEntity.Components.Add(component);
        }
    }
}
