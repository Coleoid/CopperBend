using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CopperBend.Contract
{
    //0.1
    public interface IDestroyable
    {
        int Health { get; }
        void Heal(int amount);
        void Hurt(int amount);
    }

    public interface ICanAct
    {
        Action<IControlPanel> GetNextAction();
    }

    public interface IActor : IDestroyable, ICanAct
    {
        Point Point { get; }
        string Name { get; set; }
        int Awareness { get; set; }
        IAreaMap Map { get; set; }

        ICommandSource CommandSource { get; set; }

        //IDefenseAspect DefenseAspect { get; set; }
        void MoveTo(Point point);
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
