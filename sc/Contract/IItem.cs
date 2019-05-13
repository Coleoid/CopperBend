using Color = Microsoft.Xna.Framework.Color;
using GoRogue;

namespace CopperBend.Contract
{
    public interface IItem : IHasID
    {
        string Name { get; }
        int Quantity { get; set; }

        Color Foreground { get; }
        int Glyph { get; }

        Coord Location { get; }
        void MoveTo(Coord location);

        bool IsUsable { get; }
        void ApplyTo(Coord position, IControlPanel controls, ILogWindow output, CmdDirection direction);

        bool IsConsumable { get; }
        string ConsumeVerb { get; }
        string Adjective { get; set; }

        bool SameThingAs(IItem item);
    }

    public interface ISeed : IItem
    { }

    public enum PlantType
    {
        Boomer,
        Healer,
        Thornfriend,
    }
}
