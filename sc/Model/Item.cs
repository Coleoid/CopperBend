using Color = Microsoft.Xna.Framework.Color;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Fabric;
using YamlDotNet.Serialization;

namespace CopperBend.Model
{
    public class Item : IItem
    {
        #region My IHasID
        public static void SetIDGenerator(IDGenerator generator)
        {
            IDGenerator = generator;
        }
        private static IDGenerator IDGenerator { get; set; }
        public uint ID { get; private set; }
        #endregion

        public string ItemType { get; set; } = "Item";
        public ComponentContainer Aspects { get; set; }

        public Item(int quantity = 1, uint id = uint.MaxValue)
        {
            ID = (id == uint.MaxValue ? IDGenerator.UseID() : id);
            Quantity = quantity;

            AttackMethod = new AttackMethod("physical.impact.blunt", "1d4");
            Aspects = new ComponentContainer();
        }

        public Item()
            : this(1, uint.MaxValue)
        {
            //AttackMethod = new AttackMethod("physical.impact.blunt", "1d4");
            //Aspects = new ComponentContainer();
        }

        public IAttackMethod AttackMethod { get; set; }


        public Color Foreground { get; set; }
        public int Glyph { get; set; }

        public virtual string Name { get; set; }
        public virtual string Adjective { get; set; } = string.Empty;
        public int Quantity { get; set; }

        public void MoveTo(int x, int y)
        {
            //TODO: Location = (x, y);
        }

        public void MoveTo(Coord location)
        {
            //TODO: Location = location;
        }

        [YamlIgnore]
        public bool IsUsable => Aspects.HasComponent<IUsable>();
        [YamlIgnore]
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

            //INPROG: Move Item.SplitFromStack into... Equipper?
            //Item newStack = Equipper.BuildItem(ItemType, quantity);
            Item newStack = null;
            Quantity -= quantity;

            return newStack;
        }
    }
}
