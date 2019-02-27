using CopperBend.MapUtil;

namespace CopperBend.App
{
    public interface IItem : IDrawable
    {
        string Name { get; }
        int Quantity { get; set; }
        Point Point { get; }
        void MoveTo(Point point);

        bool IsUsable { get; }
        void ApplyTo(ITile tile, IControlPanel controls, IMessageOutput output, CmdDirection direction);

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
