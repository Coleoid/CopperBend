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
        void ApplyTo(ITile tile, IAreaMap map, IControlPanel controls);
    }

    public interface ISeed : IItem
    { }

    public enum SeedType
    {
        Boomer,
        Healer,
        Thornfriend,
    }
}
