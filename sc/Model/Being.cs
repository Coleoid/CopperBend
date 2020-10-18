using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using SadConsole.Components;
using GoRogue;
using YamlDotNet.Serialization;
using CopperBend.Contract;
using SadConsole.Entities;

#pragma warning disable CA2227 // YAML s'zn wants collection setters
namespace CopperBend.Model
{
    public class Being : CbEntity, IBeing, IEntityInitPort, IHasID
    {
        public virtual string BeingType { get; set; } = "Being";
        public string Name { get; set; }

        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Energy { get; set; }
        public int MaxEnergy { get; set; }

        [YamlIgnore]
        public IEntity Entity { get => SadConEntity; }

        public Color Foreground { get; set; }
        public Color Background { get; set; }
        public int Glyph { get; set; }
        private IBeingMap Map { get; set; }
        public StrategyStyle StrategyStyle { get; set; }
        public Dictionary<string, string> StrategyStorage { get; set; }

        public IItem WieldedTool { get; internal set; }
        public IItem Gloves { get; internal set; }
        public bool IsPlayer { get; set; }
        public int Awareness { get; set; }
        public void Heal(int amount) => Health = Math.Min(Health + amount, MaxHealth);
        public void Hurt(int amount) => Health -= amount;


        //  Inventory has extra game effects, so stop myself from
        // accidentally manipulating the inventory from generic collection API.
#pragma warning disable CA2227 // Allow collection setters for YAML s'zn
        public List<IItem> InventoryPersistenceList { get; set; }
#pragma warning restore CA2227 // Allow collection setters for YAML s'zn


        public Being()
            : base(uint.MaxValue)
        {
            InventoryPersistenceList = new List<IItem>();
            StrategyStorage = new Dictionary<string, string>();
        }

        public Being(Guid blocker, Color foreground, Color background, int glyph, uint id = uint.MaxValue)
            : base(id)
        {
            Health = MaxHealth = 20;
            Energy = MaxEnergy = 140;
            Awareness = 6;

            InventoryPersistenceList = new List<IItem>();
            Foreground = foreground;
            Background = background;
            Glyph = glyph;
            StrategyStyle = StrategyStyle.DoNothing;
            StrategyStorage = new Dictionary<string, string>();
        }

        [YamlIgnore]
        public SadConsole.Console Console { get => (SadConsole.Console)SadConEntity; }

        //  IBeing
        //  This all seems like it ought to be in BeingMap.  Or PiecePusher.
        public void MoveTo(IBeingMap map)
        {
            if (map != Map)
            {
                Map?.Remove(this);
            }
            if (map != null)
            {
                var mapPosition = map.GetPosition(this);
                if (mapPosition.X == int.MinValue)
                {
                    map.Add(this, this.GetPosition());
                }
            }
            Map = map;
        }

        public void MoveTo(Coord coord)
        {
            SadConEntity.Position = coord;
            Map.Move(this, coord);
        }

        public Coord GetPosition()
        {
            return SadConEntity.Position;
        }

        //0.1  Wrong place.  Collect a volume of standard effects?
        private readonly AttackEffect lifeChampion = new AttackEffect
        {
            Type = "vital.nature",
            DamageRange = "2d3+4", // 6-10
        };

        //  IAttacker
        public IAttackMethod GetAttackMethod(IDefender defender)
        {
            //0.2:  Different beings get different natural weaponry.  No more wolves with fists.
            var attack = WieldedTool?.AttackMethod ?? new AttackMethod("physical.impact.blunt", "1d3+2");
            if (defender is AreaRot)
            {
                if (IsPlayer && WieldedTool == null && Gloves == null)
                {
                    attack.AddEffect(lifeChampion);
                    //TODO: can message from Being: Message(this, Messages.BarehandRotDamage);
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

            if (IsPlayer)
            {
                //0.1: commented out short term, whil testing character death
                //defenseMethod.Resistances.Add("vital.rot", "4/5");
            }
            defenseMethod.Resistances.Add("default", "1/5");

            return defenseMethod;
        }

        public List<IModifier> GetDefenseModifiers(IAttacker attacker, IAttackMethod method)
        {
            return new List<IModifier>();
        }

        [YamlIgnore]
        public IReadOnlyCollection<IItem> Inventory
        {
            get => new ReadOnlyCollection<IItem>(InventoryPersistenceList);
        }

        public void AddToInventory(IItem item)
        {
            //0.2.INV  limit stack size of some items
            var existingItem = Inventory
                .FirstOrDefault(i => i.StacksWith(item));
            if (existingItem == null)
                InventoryPersistenceList.Add(item);
            else
                existingItem.Quantity += item.Quantity;
                // and 'delete' item?
        }

        public bool HasInInventory(IItem item)
        {
            return InventoryPersistenceList.Any(i => i == item);
        }

        public IItem RemoveFromInventory(int inventorySlot, int quantity = 0)
        {
            if (inventorySlot >= InventoryPersistenceList.Count) return null;

            IItem item = InventoryPersistenceList.ElementAt(inventorySlot);
            return RemoveFromInventory(item, quantity);
        }

        public IItem RemoveFromInventory(IItem item, int quantity = 0)
        {
            if (!InventoryPersistenceList.Contains(item)) return null;

            if (quantity == 0 || quantity >= item.Quantity)
            {
                InventoryPersistenceList.Remove(item);
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
            if (item != null && !InventoryPersistenceList.Any(i => i == item))
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
                Agent = this,
            };
        }

        //public virtual void GiveCommand()
        //{
        //    Strategy.GiveCommand(this);
        //}

        public void Fatigue(int amount)
        {
            Energy -= amount;
            //0.1: as energy hits 0, actions prevented.
            //0.2: as energy nears 0, chance of ill effects.
        }

        public void AddComponent(IConsoleComponent component)
        {
            SadConEntity.Components.Add(component);
        }
    }
}
