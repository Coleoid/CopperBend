using System.Collections.Generic;
using Color = Microsoft.Xna.Framework.Color;
using GoRogue;
using CopperBend.Model;
using CopperBend.Fabric;

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

        IEnumerable<Item> Inventory { get; }
        void AddToInventory(Item item);
        Item RemoveFromInventory(int inventorySlot);
        Item RemoveFromInventory(Item item);
        IEnumerable<Item> ReachableItems();

        Item WieldedTool { get; }
        Item Gloves { get; }
        void Wield(Item item);
        bool HasClearedBlightBefore { get; set; }
    }
}
