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
            Inventory = new List<IItem>();
            Health = 6;
            _behavior = new StandardMoveAndAttack();
            Strategy = _behavior.Act;
            Awareness = 6;
        }

        public int Health { get; protected set; }

        //  IActor
        public string Name { get; set; }
        public int Awareness { get; set; }
        public List<IItem> Inventory { get; private set; }

        public void Damage(int amount)
        {
            Health -= amount;
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

        public Func<ScheduleEntry, IAreaMap, IActor, ScheduleEntry> Strategy { get; private set; }

        internal void AddToInventory(IItem topItem)
        {
            //0.1 everything stacks
            var existingItem = Inventory.FirstOrDefault(i => i.Name == topItem.Name);
            if (existingItem == null)
                Inventory.Add(topItem);
            else
                existingItem.Quantity += topItem.Quantity;
        }
    }
}
