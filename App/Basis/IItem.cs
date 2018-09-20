using RogueSharp;

namespace CopperBend.App
{
    public interface IItem : IDrawable, ICoord
    {
        string Name { get; }
    }
}
