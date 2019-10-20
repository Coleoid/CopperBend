using Color = Microsoft.Xna.Framework.Color;
using GoRogue;

namespace CopperBend.Contract
{
    public interface IItem : IHasID
    {
        string Name { get; }
        string ItemType { get; set; }
        int Quantity { get; set; }

        Color Foreground { get; }
        int Glyph { get; }

        Coord Location { get; set; }
        void MoveTo(Coord location);

        bool IsUsable { get; }
        void ApplyTo(Coord position, IControlPanel controls, ILogWindow output, CmdDirection direction);

        bool IsIngestible { get; }
        string Adjective { get; set; }
        IAttackMethod AttackMethod { get; }

        ComponentContainer Aspects { get; set; }

        bool StacksWith(IItem item);
        IItem SplitFromStack(int quantity = 1);
    }
}
