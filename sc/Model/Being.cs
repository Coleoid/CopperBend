using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using GoRogue;
using CopperBend.Contract;

namespace CopperBend.Model
{
    public class Being : CbEntity, IBeing, IGetSadCon, IHasID
    {
        public virtual string BeingType { get; set; } = "Being";
        public static IEntityFactory EntityFactory { get; set; }

        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public char Symbol { get; set; }

        public Color Foreground { get; set; }
        public Color Background { get; set; }
        public int Glyph { get; set; }

        public Being(Color foreground, Color background, int glyph, uint id = uint.MaxValue)
            : base(id)
        {
            Health = MaxHealth = 6;
            Awareness = 6;

            InventoryList = new List<IItem>();
            Foreground = foreground;
            Background = background;
            Glyph = glyph;
            SadConEntity = EntityFactory.GetSadCon(this);
        }

        public Point Position { get => SadConEntity.Position; set => SadConEntity.Position = value; }

        public SadConsole.Console Console { get => (SadConsole.Console)SadConEntity; }

        //  IBeing
        public void MoveTo(Coord point)
        {
            SadConEntity.Position = point;
        }

        public int Awareness { get; set; }
        //public IAreaMap Map { get; set; }

        //  IDestroyable
        public void Heal(int amount) => Health = Math.Min(Health + amount, MaxHealth);
        public void Hurt(int amount) => Health -= amount;

        public ICommandSource CommandSource { get; set; }


        //0.1  Wrong place.  Collect a volume of standard effects?
        readonly AttackEffect lifeChampion = new AttackEffect
        {
            DamageType = DamageType.Nature_itself,
            DamageRange = "2d3+4" // 6-10
        };

        //  IAttacker
        public IAttackMethod GetAttackMethod(IDefender defender)
        {
            var attack = WieldedTool?.AttackMethod ?? new AttackMethod();
            if (defender is AreaBlight)
            {
                if (IsPlayer && WieldedTool == null && Gloves == null)
                {
                    attack.AddEffect(lifeChampion);
                    //TODO: can message from Being: Message(this, Messages.BarehandBlightDamage);
                }
            }

            return attack;
        }
        public List<IModifier> GetAttackModifiers(IDefender defender, IAttackMethod method)
        {
            throw new NotImplementedException();
        }

        //  IDefender
        public IDefenseMethod GetDefenseMethod(IAttackMethod method)
        {
            throw new NotImplementedException();
        }
        public List<IModifier> GetDefenseModifiers(IAttacker attacker, IAttackMethod method)
        {
            throw new NotImplementedException();
        }


        public IItem WieldedTool { get; internal set; }
        public IItem Gloves { get; internal set; }

        public bool IsPlayer { get; set; }
        
        public bool HasClearedBlightBefore { get; set; }
        
        //  Inventory has extra game effects, so I want to be sure I
        //  don't casually add/remove directly from the list, from outside.
        private List<IItem> InventoryList;
        public IReadOnlyCollection<IItem> Inventory
        {
            get => new ReadOnlyCollection<IItem>(InventoryList);
        }

        //public string Name { get => ScEntity.Name; set => ScEntity.Name = value; }
        public string Name { get; set; }
        
        public void AddToInventory(IItem item)
        {
            //0.2.INV  limit stack size of some items
            var existingItem = Inventory
                .FirstOrDefault(i => i.StacksWith(item));
            if (existingItem == null)
                InventoryList.Add(item);
            else
                existingItem.Quantity += item.Quantity;
                // and 'delete' item?
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
                Action = ScheduleAction.GetCommand,
                Offset = offset,
                Agent = this
            };
        }

        public virtual void GiveCommand()
        {
            CommandSource.GiveCommand(this);
        }
    }
}
