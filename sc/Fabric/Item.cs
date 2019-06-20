using Color = Microsoft.Xna.Framework.Color;
using GoRogue;
using CopperBend.Contract;
using System;
using CopperBend.Model;

namespace CopperBend.Fabric
{
    public class Item : IHasID
    {
        public Item(ItemProto prototype, string name = null, int quantity = 1, uint id = uint.MaxValue)
        {
            ID = (id == uint.MaxValue ? IDGenerator.UseID() : id);
            TakeValuesFromPrototype(prototype);
        }
        //  And a similar ctor taking a string, for extending prototypes in data

        public void TakeValuesFromPrototype(ItemProto proto)
        {
            Item protoItem = PrototypeOf(proto);
        }

        internal T GetComponent<T>()
        {
            throw new NotImplementedException();
        }

        private Item() { }

        private Item PrototypeOf(ItemProto proto)
        {
            switch (proto)
            {
            case ItemProto.Unset:
                throw new Exception("Must choose a prototype (or None)");

            case ItemProto.None:
                return new Item
                {
                    Name = "thing",
                    Quantity = 2,
                    Adjective = "defaulty",
                    Foreground = Color.Beige,
                    Glyph = '*',
                    IsUsable = false,
                    Location = (0, 0)
                };
            case ItemProto.Knife:
                return new Item
                {
                    Name = "knife",
                    Quantity = 1,
                    Foreground = Color.SteelBlue,
                    Glyph = '-',
                    IsUsable = true,
                    Location = (0, 0)
                };
            case ItemProto.Rock:
                return new Item
                {
                    Name = "rock",
                    Quantity = 1,
                    Foreground = Color.DarkGray,
                    Glyph = ',',
                    IsUsable = true,
                    Location = (0, 0)
                };

            default:
                throw new Exception("Need to extend .PrototypeOf()");
            }
        }

        public Item(Coord location, int quantity = 1, bool isUsable = false, uint id = uint.MaxValue)
        {
            ID = (id == uint.MaxValue ? IDGenerator.UseID() : id);
            Location = location;
            Quantity = quantity;
            IsUsable = isUsable;
        }

        public static IDGenerator IDGenerator;
        public uint ID { get; set; }

        public Color Foreground { get; set; }
        public int Glyph { get; set; }

        public string Name { get; set; }
        public string Adjective { get; set; } = string.Empty;
        public int Quantity { get; set; }

        public Coord Location { get; protected set; }

        public void MoveTo(int x, int y)
        {
            Location = (x, y);
        }

        public void MoveTo(Coord location)
        {
            Location = location;
        }

        public bool IsUsable { get; set; }

        public void ApplyTo(Coord position, IControlPanel controls, ILogWindow output, CmdDirection direction)
        {
            //output.Add($"Can't use a {Name} on {tile.TileType} to my {direction}.");
        }

        public bool IsConsumable => false;

        public string ConsumeVerb => "eat";

        public bool StacksWith(Item item)
        {
            return Name == item.Name && Adjective == item.Adjective && Glyph == item.Glyph;
        }

        public override bool Equals(object other)
        {
            if (other == null) return false;
            if (other is Item item)
            {
                return ID == item.ID && StacksWith(item) && Quantity == item.Quantity;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, Name, Glyph);
        }
    }

    public enum ItemProto
    {
        Unset = 0,
        None,
        Seed,
        Knife,
        Hoe,
        Rock
    }
}
