using System;
using System.Collections.Generic;
using System.Linq;
using CopperBend.App.Behaviors;
using CopperBend.MapUtil;
using RLNET;

namespace CopperBend.App.Model
{
    public class Actor : IActor
    {
        private IBehavior _behavior;
        public Actor(Point point)
        {
            Point = point;
            Health = 6;
            Awareness = 6;

            InventoryList = new List<IItem>();
            _behavior = new StandardMoveAndAttack();
        }

        //  IComponent
        public IActor Entity { get => this; }

        //  IDrawable
        public Point Point { get; protected set; }
        public RLColor ColorForeground { get; set; }
        public char Symbol { get; set; }
        public void MoveTo(Point point)
        {
            Point = point;
        }

        //  IActor
        public string Name { get; set; }
        public int Awareness { get; set; }
        public IAreaMap Map { get; set; }

        //  IHealAndHurt
        public int Health { get; set; }
        public void Heal(int amount) => Health += amount;
        public void Hurt(int amount) => Health -= amount;
        public IDefenseAspect DefenseAspect { get; set; }

        public Action<IControlPanel, ScheduleEntry> NextAction => _behavior.NextAction;

        public IItem WieldedTool { get; internal set; }

        //  Inventory has extra game effects, so I want to be sure I
        //  don't casually add/remove directly from the list, from outside.
        private List<IItem> InventoryList;
        public IEnumerable<IItem> Inventory
        {
            get => InventoryList;
        }

        public void AddToInventory(IItem item)
        {
            //0.1 everything stacks
            var existingItem = Inventory
                .FirstOrDefault(i => i.SameThingAs(item));
            if (existingItem == null)
                InventoryList.Add(item);
            else
                existingItem.Quantity += item.Quantity;
        }

        public IItem RemoveFromInventory(int inventorySlot)
        {
            if (inventorySlot >= InventoryList.Count()) return null;

            IItem item = InventoryList.ElementAt(inventorySlot);
            InventoryList.RemoveAt(inventorySlot);
            if (WieldedTool == item) WieldedTool = null;

            return item;
        }

        public IItem RemoveFromInventory(IItem item)
        {
            if (!InventoryList.Contains(item)) return null;

            InventoryList.Remove(item);
            if (WieldedTool == item)
                WieldedTool = null;

            return item;
        }

        public void Wield(IItem item)
        {
            WieldedTool = item;
            if (item != null && !InventoryList.Any(i => i == item))
                AddToInventory(item);
        }

        public IEnumerable<IItem> ReachableItems()
        {
            return Map.Items.Where(i => i.Point.Equals(Point));
        }

    }
}
