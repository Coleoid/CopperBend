using System;
using System.Collections.Generic;
using Color = Microsoft.Xna.Framework.Color;
using GoRogue;

namespace CopperBend.Contract
{
    public interface IItem : IHasID
    {
        string Name { get; }
        string ItemType { get; }
        int Quantity { get; set; }

        Color Foreground { get; }
        int Glyph { get; }

        Coord Location { get; set; }
        void MoveTo(Coord location);

        bool IsUsable { get; }
        void ApplyTo(Coord position, IControlPanel controls, ILogWindow output, CmdDirection direction);

        bool IsConsumable { get; }
        string ConsumeVerb { get; }
        string Adjective { get; set; }
        IAttackMethod AttackMethod { get; }

        bool StacksWith(IItem item);
    }

    public interface ISeed : IItem
    { }

    public class PlantDetails
    {
        public uint ID;
        public string MainName { get; set; }
        public int GrowthTime { get; set; }
        public bool SeedKnown;
        public bool FruitKnown;
        public string SeedAdjective;
        public string FruitAdjective;
        public string SeedDescriptionAsKnown => $"{(SeedKnown ? MainName : SeedAdjective)} seed";
        public string FruitDescriptionAsKnown => FruitKnown ? MainName : $"{FruitAdjective} fruit";
        public List<(PlantPart, PlantUse, string)> Uses;  //0.+
    }

    public enum PlantPart  //0.+
    {
        Unset = 0,
        Root,
        Stem,
        Leaf,
        Flower,
        Fruit,
        Seed
    }

    [Flags]
    public enum PlantUse  //0.+
    {
        Unset = 0,
        Food = 1,
        Medicine = 2,
        Toxin = 4,
        Textile = 8,
        Flavor = 16,
        Beauty = 32,
        Magic = 64,
        Symbolism = 128
    }
}
