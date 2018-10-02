using System;
using System.Collections.Generic;
using System.Linq;
using CopperBend.App.Behaviors;
using RLNET;

namespace CopperBend.App.Model
{
    public class Actor : IActor
    {
        private IBehavior _behavior;
        public Actor(int x, int y)
        {
            X = x;
            Y = y;
            InventoryList = new List<IItem>();
            Health = 6;
            _behavior = new StandardMoveAndAttack();
            Strategy = _behavior.Act;
            Awareness = 6;
        }

        //  IDrawable
        public RLColor Color { get; set; }
        public char Symbol { get; set; }
        public void MoveTo(int x, int y)
        {
            X = x;
            Y = y;
        }

        //  ICoord
        public int X { get; protected set; }
        public int Y { get; protected set; }


        public int Health { get; protected set; }

        //  IActor
        public string Name { get; set; }
        public int Awareness { get; set; }

        private List<IItem> InventoryList;
        public IEnumerable<IItem> Inventory
        {
            get => InventoryList;
        }

        public void Damage(int amount)
        {
            Health -= amount;
        }

        public Func<ScheduleEntry, IGameState, ScheduleEntry> Strategy { get; private set; }
        public IItem WieldedTool { get; internal set; }

        public void AddToInventory(IItem topItem)
        {
            //0.1 everything stacks
            var existingItem = Inventory
                .FirstOrDefault(i => i.SameThingAs(topItem));
            if (existingItem == null)
                InventoryList.Add(topItem);
            else
                existingItem.Quantity += topItem.Quantity;
        }

        public IItem RemoveFromInventory(int inventorySlot)
        {
            var item = InventoryList.ElementAt(inventorySlot);
            InventoryList.RemoveAt(inventorySlot);

            if (WieldedTool == item)
            {
                Console.Out.WriteLine($"Note:  No longer wielding the {item.Name}.");
                WieldedTool = null;
            }

            return item;
        }

        public void Wield(IItem item)
        {
            WieldedTool = item;
        }
    }
}
