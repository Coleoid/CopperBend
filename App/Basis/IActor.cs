using System;
using System.Collections.Generic;
using RogueSharp;

namespace CopperBend.App
{
    public interface IActor : IDrawable, ICoord
    {
        string Name { get; set; }
        int Awareness { get; set; }

        IEnumerable<IItem> Inventory { get; }
        void AddToInventory(IItem topItem);

        int Health { get; }
        void Damage(int v);

        Func<ScheduleEntry, IAreaMap, IActor, ScheduleEntry> Strategy { get; }
    }
}
