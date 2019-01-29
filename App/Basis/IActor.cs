using System;
using System.Collections.Generic;
using CopperBend.MapUtil;

namespace CopperBend.App
{
    public interface IComponent
    {
        IActor Entity { get; }
    }

    public interface IHealAndHurt : IComponent
    {
        int Health { get; }
        void Heal(int amount);
        void Hurt(int amount);
    }

    public interface ICanAct
    {
        void NextAction(IControlPanel controls);
    }

    public interface IActor : IDrawable, IHealAndHurt, ICanAct
    {
        ICommandSource CommandSource { get; set; }
        Point Point { get; }
        string Name { get; set; }
        int Awareness { get; set; }
        IAreaMap Map { get; set; }
        IDefenseAspect DefenseAspect { get; set; }
        void MoveTo(Point point);
        IItem WieldedTool { get; }
        void Wield(IItem item);

        IEnumerable<IItem> Inventory { get; }
        void AddToInventory(IItem item);
        IItem RemoveFromInventory(int inventorySlot);
        IItem RemoveFromInventory(IItem item);

        IEnumerable<IItem> ReachableItems();

    }
}
