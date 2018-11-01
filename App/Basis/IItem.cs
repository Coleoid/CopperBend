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
        void ApplyTo(ITile tile, IControlPanel controls, Direction direction);

        bool IsConsumable { get; }
        string ConsumeVerb { get; }
        string Adjective { get; set; }

        void Consume(IControlPanel controls);

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
