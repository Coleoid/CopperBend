using System.Collections.Generic;
using Color = Microsoft.Xna.Framework.Color;
using GoRogue;
using SadConsole.Components;
using SadConsole.Entities;

namespace CopperBend.Contract
{
    public interface IBeing : IDelible, IScheduleAgent, IHasID, IAttacker, IDefender
    {
        string BeingType { get; set; }
        IEntity Entity { get; }
        SadConsole.Console Console { get; }
        StrategyStyle StrategyStyle { get; set; }
        Dictionary<string, string> StrategyStorage { get; }

        new string Name { get; set; }  // wrasslin' with IDelible
        Color Foreground { get; }
        Color Background { get; }
        int Glyph { get; }

        int Awareness { get; set; }
        bool IsPlayer { get; set; }
        IItem WieldedTool { get; }
        IItem Gloves { get; }

        void Wield(IItem item);
        void Fatigue(int amount);
        void MoveTo(IBeingMap map);
        void MoveTo(Coord position);
        Coord GetPosition();

        IReadOnlyCollection<IItem> Inventory { get; }
        void AddToInventory(IItem item);
        IItem RemoveFromInventory(int inventorySlot, int quantity = 0);
        IItem RemoveFromInventory(IItem item, int quantity = 0);
        bool HasInInventory(IItem item);
        IEnumerable<IItem> ReachableItems();

        void AddComponent(IConsoleComponent component);
    }
}
