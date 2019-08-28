using Color = Microsoft.Xna.Framework.Color;
using SadConsole.Components;

namespace CopperBend.Model
{
    public class Player : Being
    {
        public Player(Color foreground, Color background, int glyph = '@')
            : base(foreground, background, glyph)
        {
            IsPlayer = true;
        }

        internal void AddComponent(IConsoleComponent component)
        {
            ScEntity.Components.Add(component);
        }
    }
}
