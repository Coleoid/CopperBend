using Color = Microsoft.Xna.Framework.Color;
using GoRogue;
using CopperBend.Contract;
using System.Linq;
using System;
using CopperBend.Model.Aspects;

namespace CopperBend.Model
{
    public class Item : IItem
    {
        public static IDGenerator IDGenerator;
        public virtual string ItemType { get; } = "Item";
        public uint ID { get; private set; }

        public Item(Coord location, int quantity = 1, bool isUsable = false, uint id = uint.MaxValue)
        {
            ID = (id == uint.MaxValue ? IDGenerator.UseID() : id);
            Location = location;
            Quantity = quantity;
            IsUsable = isUsable;

            AttackMethod = new AttackMethod("physical.impact.blunt", "1d4");
            Aspects = new ComponentContainer();
        }

        public IAttackMethod AttackMethod { get; set; }


        public Color Foreground { get; set; }
        public int Glyph { get; set; }

        public virtual string Name { get; set; }
        public virtual string Adjective { get; set; } = string.Empty;
        public int Quantity { get; set; }

        public Coord Location { get; set; }

        public void MoveTo(int x, int y)
        {
            Location = (x, y);
        }

        public void MoveTo(Coord location)
        {
            Location = location;
        }

        public bool IsUsable { get; set; }

        public virtual void ApplyTo(Coord position, IControlPanel controls, ILogWindow output, CmdDirection direction)
        {
            //output.Add($"Can't use a {Name} on {tile.TileType} to my {direction}.");
        }

        public void AddAspect(object aspect)
        {
            Aspects.AddComponent(aspect);
        }

        public Use AddUse(string verbPhrase, UseTargetFlags targets)
        {
            IUsable usable = Aspects.GetComponent<IUsable>();
            if (usable == null)
            {
                usable = new Usable();
                Aspects.AddComponent(usable);
            }
            var use = new Use(verbPhrase, targets);
            usable.Uses.Add(use);
            return use;
        }

        public virtual bool IsIngestible 
        {
            get 
            {
                // this is squishy, may eventually be noticeably slow
                return Aspects.GetComponents<IUsable>()
                    .Any(ub => ub.Uses.Any(u => u.VerbPhrase == "eat" || u.VerbPhrase == "drink"));
            }
        }

        public virtual bool StacksWith(IItem item)
        {
            return Name == item.Name
                && ItemType == item.ItemType;
        }

        public ComponentContainer Aspects { get; set; }
    }
}
