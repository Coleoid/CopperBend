using Microsoft.Xna.Framework;

namespace CopperBend.Contract
{
    public interface IDrawable
    {
        Color ColorForeground { get; }
        char Symbol { get; }
    }
}
