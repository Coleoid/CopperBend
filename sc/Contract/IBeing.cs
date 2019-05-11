using System.Collections.Generic;
using GoRogue;

namespace CopperBend.Contract
{
    //0.1
    public interface IDestroyable
    {
        int MaxHealth { get; }
        int Health { get; }
        void Heal(int amount);
        void Hurt(int amount);
    }

    public interface IScheduleAgent
    {
        ScheduleEntry GetNextEntry();
        ScheduleEntry GetNextEntry(int offset);
    }

    public interface IBeing : IDestroyable, IScheduleAgent, IHasID
    {
        Microsoft.Xna.Framework.Point Position { get; set; }  // Awkward bit to match SadConsole.Console...
        string Name { get; set; }
        int Awareness { get; set; }

        ICommandSource CommandSource { get; set; }

        void MoveTo(Coord position);
        IItem WieldedTool { get; }
        void Wield(IItem item);

        IEnumerable<IItem> Inventory { get; }
        bool IsPlayer { get; set; }

        void AddToInventory(IItem item);
        IItem RemoveFromInventory(int inventorySlot);
        IItem RemoveFromInventory(IItem item);

        IEnumerable<IItem> ReachableItems();
    }
}
