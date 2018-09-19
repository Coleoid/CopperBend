using RLNET;
using RogueSharp;

namespace CopperBend.App
{
    public interface IDrawable : ICoord
    {
        RLColor Color { get; }
        char Symbol { get; }

        void MoveTo(int x, int y);
    }
}
