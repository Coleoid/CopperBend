using System.Collections.Generic;
using Color = Microsoft.Xna.Framework.Color;
using GoRogue;

namespace CopperBend.Contract
{
    public interface IBeing : IDestroyable, IScheduleAgent, IHasID
    {
        string Name { get; set; }
        int Awareness { get; set; }
        bool IsPlayer { get; set; }

        Color Foreground { get; }
        int Glyph { get; }

        ICommandSource CommandSource { get; set; }

        // This declaration left awkward intentionally, for SadConsole.Console...
        Microsoft.Xna.Framework.Point Position { get; set; }
        void MoveTo(Coord position);

        IEnumerable<IItem> Inventory { get; }
        void AddToInventory(IItem item);
        IItem RemoveFromInventory(int inventorySlot);
        IItem RemoveFromInventory(IItem item);
        IEnumerable<IItem> ReachableItems();

        IItem WieldedTool { get; }
        IItem Gloves { get; }
        void Wield(IItem item);
        bool HasClearedBlightBefore { get; set; }
    }
}
