using System.Collections.Generic;
using Color = Microsoft.Xna.Framework.Color;
using GoRogue;

namespace CopperBend.Contract
{
    public interface IBeing : IDelible, IScheduleAgent, IHasID, IAttacker, IDefender
    {
        Color Foreground { get; }
        Color Background { get; }
        int Glyph { get; }
        string BeingType { get; set; }

        int Awareness { get; set; }
        bool IsPlayer { get; set; }

        ICommandSource CommandSource { get; set; }

        SadConsole.Console Console { get; }

        void MoveTo(IBeingMap map);
        void MoveTo(Coord position);
        Coord GetPosition();

        IReadOnlyCollection<IItem> Inventory { get; }
        void AddToInventory(IItem item);
        IItem RemoveFromInventory(int inventorySlot, int quantity = 0);
        IItem RemoveFromInventory(IItem item, int quantity = 0);
        bool HasInInventory(IItem item);
        IEnumerable<IItem> ReachableItems();

        IItem WieldedTool { get; }
        IItem Gloves { get; }

        void Wield(IItem item);
        void GiveCommand();
        void Fatigue(int amount);
    }
}
