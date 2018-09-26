using RogueSharp;

namespace CopperBend.App
{
    public interface IItem : IDrawable, ICoord
    {
        string Name { get; }
        int Quantity { get; set; }
        bool IsUsable { get; }
    }
}
