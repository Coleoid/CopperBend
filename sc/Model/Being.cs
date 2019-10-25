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
        public int Energy { get; set; }
        public int MaxEnergy { get; set; }
        public char Symbol { get; set; }

        public Color Foreground { get; set; }
        public Color Background { get; set; }
        public int Glyph { get; set; }

        public Being(Color foreground, Color background, int glyph, uint id = uint.MaxValue)
            : base(id)
        {
            Health = MaxHealth = 20;
            Energy = MaxEnergy = 140;
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
            Type = "vital.nature",
            DamageRange = "2d3+4" // 6-10
        };

        //  IAttacker
        public IAttackMethod GetAttackMethod(IDefender defender)
        {
            //0.2:  Different beings get different natural weaponry.  No more wolves with fists.
            var attack = WieldedTool?.AttackMethod ?? new AttackMethod("physical.impact.blunt", "1d3+2");
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
        public IDefenseMethod GetDefenseMethod(IAttackMethod attackMethod)
        {
            var defenseMethod = new DefenseMethod();

            if (IsPlayer) //0.1  Nice. and. Crispy.
            {
                defenseMethod.Resistances.Add("vital.blight", "4/5");
                defenseMethod.Resistances.Add("default", "1/5");
            }
            else
            {
                defenseMethod.Resistances.Add("default", "1/5");
            }

            return defenseMethod;
        }

        public List<IModifier> GetDefenseModifiers(IAttacker attacker, IAttackMethod method)
        {
            return new List<IModifier>();
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

        public bool HasInInventory(IItem item)
        {
            return InventoryList.Any(i => i == item);
        }

        public IItem RemoveFromInventory(int inventorySlot, int quantity = 0)
        {
            if (inventorySlot >= InventoryList.Count()) return null;

            IItem item = InventoryList.ElementAt(inventorySlot);
            return RemoveFromInventory(item, quantity);
        }

        public IItem RemoveFromInventory(IItem item, int quantity = 0)
        {
            if (!InventoryList.Contains(item)) return null;

            if (quantity == 0 || quantity >= item.Quantity)
            {
                InventoryList.Remove(item);
                if (WieldedTool == item)
                    WieldedTool = null;
                return item;
            }

            var returnedPortion = item.SplitFromStack(quantity);
            return returnedPortion;
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

        public void Fatigue(int amount)
        {
            Energy -= amount;
            //0.1: as energy hits 0, actions prevented.
            //0.2: as energy nears 0, chance of ill effects.
        }
    }
}
