using System.Collections.Generic;
using Color = Microsoft.Xna.Framework.Color;
using GoRogue;

namespace CopperBend.Contract
{
    public interface IBeing : IDestroyable, IScheduleAgent, IHasID, IAttacker, IDefender
    {
        string Name { get; set; }
        int Awareness { get; set; }
        bool IsPlayer { get; set; }

        Color Foreground { get; }
        Color Background { get; }
        int Glyph { get; }
        string BeingType { get; set; }

        ICommandSource CommandSource { get; set; }

        // Left awkward intentionally, avoiding clash with SadConsole.Console
        Microsoft.Xna.Framework.Point Position { get; set; }
        void MoveTo(Coord position);

        IReadOnlyCollection<IItem> Inventory { get; }
        void AddToInventory(IItem item);
        IItem RemoveFromInventory(int inventorySlot);
        IItem RemoveFromInventory(IItem item);
        IEnumerable<IItem> ReachableItems();

        IItem WieldedTool { get; }
        IItem Gloves { get; }

        void Wield(IItem item);
        bool HasClearedBlightBefore { get; set; }  //TODO: Subsume into messaging
        void GiveCommand();
    }
}
