using Color = Microsoft.Xna.Framework.Color;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Fabric;

namespace CopperBend.Model
{
    public class Item : IItem
    {
        public static IDGenerator IDGenerator { get; set; }
        public string ItemType { get; set; } = "Item";
        public uint ID { get; private set; }
        public ComponentContainer Aspects { get; set; }

        public Item(Coord location, int quantity = 1, uint id = uint.MaxValue)
        {
            ID = (id == uint.MaxValue ? IDGenerator.UseID() : id);
            Location = location;
            Quantity = quantity;

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

        public bool IsUsable => Aspects.HasComponent<IUsable>();
        public virtual bool IsIngestible => Aspects.HasComponent<IIngestible>();

        public virtual void ApplyTo(Coord position, IControlPanel controls, ILogWindow output, CmdDirection direction)
        {
            //output.Add($"Can't use a {Name} on {tile.TileType} to my {direction}.");
        }

        public void AddAspect(object aspect)
        {
            Aspects.AddComponent(aspect);
        }


        public virtual bool StacksWith(IItem item)
        {
            return Name == item.Name
                && ItemType == item.ItemType;
        }

        public IItem SplitFromStack(int quantity = 1)
        {
            Guard.Against(Quantity < 1, $"Somehow there's no {Name} here.");
            Guard.Against(Quantity < quantity, $"Want {quantity} of {Name} but there are only {Quantity}.");

            Item newStack = Equipper.BuildItem(ItemType, quantity);
            Quantity -= quantity;

            return newStack;
        }
    }
}
