using CopperBend.App.Model;
using RogueSharp;

namespace CopperBend.App
{
    public interface IItem : IDrawable, ICoord
    {
        string Name { get; }
        int Quantity { get; set; }
        bool IsUsable { get; }
        //ItemType ItemType { get; }

        bool SameThingAs(IItem item);
    }

    public enum SeedType
    {
        Boomer,
        Healer,
        Thornfriend,
    }
}
