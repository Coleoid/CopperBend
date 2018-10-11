using System;
using System.Collections.Generic;
using RogueSharp;

namespace CopperBend.App
{
    public interface IActor : IDrawable, ICoord
    {
        string Name { get; set; }
        int Awareness { get; set; }
        int Health { get; }
        void AdjustHealth(int amount);

        IItem WieldedTool { get; }
        void Wield(IItem item);

        IEnumerable<IItem> Inventory { get; }
        void AddToInventory(IItem item);
        IItem RemoveFromInventory(int inventorySlot);
        IItem RemoveFromInventory(IItem item);

        Action<IControlPanel, ScheduleEntry> NextAction { get; }
    }
}
