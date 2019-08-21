using System;
using System.Collections.Generic;
using System.Linq;
using Color = Microsoft.Xna.Framework.Color;
using GoRogue;
using CopperBend.Contract;
using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Components;

namespace CopperBend.Model
{
    public abstract class CbEntity : /*SadConsole.Entities.Entity,*/ IHasID
    {
        //protected SadConsole.IEntity ScEntity = null;
        protected SadConsole.Entities.Entity ScEntity = null;

        protected CbEntity(Color foreground, Color background, int glyph, uint id = uint.MaxValue)
        {
            ID = (id == uint.MaxValue? IDGenerator.UseID() : id);
            var factory = new ScEntityFactory();
            ScEntity = factory.NewEntity(foreground, background, glyph);
        }

        #region standard IHasID
        public static IDGenerator IDGenerator;
        public uint ID { get; private set; }
        #endregion
    }

    public class Being : CbEntity, IBeing
    {
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public char Symbol { get; set; }

        public Color Foreground => ScEntity.Animation.CurrentFrame[0].Foreground;
        public Color Background => ScEntity.Animation.CurrentFrame[0].Background;
        public int Glyph => ScEntity.Animation.CurrentFrame[0].Glyph;

        protected Being(Color foreground, Color background, int glyph)
            : base(foreground, background, glyph)
        {
            Health = MaxHealth = 6;
            Awareness = 6;

            InventoryList = new List<IItem>();
        }

        internal IControlPanel Controls { get; set; }

        //  IBeing
        public void MoveTo(Coord point)
        {
            ScEntity.Position = point;
        }

        public int Awareness { get; set; }
        //public IAreaMap Map { get; set; }

        //  IDestroyable
        public void Heal(int amount) => Health = Math.Min(Health + amount, MaxHealth);
        public void Hurt(int amount) => Health -= amount;

        public ICommandSource CommandSource { get; set; }

        public IItem WieldedTool { get; internal set; }
        public IItem Gloves { get; internal set; }

        public bool IsPlayer { get; set; }
        
        public bool HasClearedBlightBefore { get; set; }
        
        internal void CmdDirection(CmdDirection direction)
        {
            //log.Debug($"got CmdDirection({direction})");
            Controls.ScheduleAgent(this, 12); //0.0
        }


        //  Inventory has extra game effects, so I want to be sure I
        //  don't casually add/remove directly from the list, from outside.
        private List<IItem> InventoryList;
        public IEnumerable<IItem> Inventory
        {
            get => InventoryList;
        }
        public string Name { get => ScEntity.Name; set => ScEntity.Name = value; }
        public Point Position { get => ScEntity.Position; set => ScEntity.Position = value; }
        public SadConsole.Console Console { get => ScEntity; }

        public void AddToInventory(IItem item)
        {
            //0.2.INV  limit stack size of some items
            var existingItem = Inventory
                .FirstOrDefault(i => i.StacksWith(item));
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
            throw new NotImplementedException();
            //return Map.Items.Where(i => i.Point.Equals(Point));
        }

        public virtual ScheduleEntry GetNextEntry()
        {
            return GetNextEntry(12);
        }

        public virtual ScheduleEntry GetNextEntry(int offset)
        {
            return new ScheduleEntry
            {
                Action = (icp) => { CommandSource.GiveCommand(this); },
                Offset = offset,
                Agent = this
            };
        }
    }

    public class Player : Being
    {
        public Player(Color foreground, Color background, int glyph = '@')
            : base(foreground, background, glyph)
        {
            IsPlayer = true;
        }

        internal void AddComponent(IConsoleComponent component)
        {
            ScEntity.Components.Add(component);
        }
    }

    public class Monster : Being
    {
        public Monster(Color foreground, Color background, int glyph = 'M') 
            : base(foreground, background, glyph)
        {
        }
    }
}
