using System;

namespace CopperBend.Contract
{
    public interface ISeed : IItem
    { }

    public enum PlantPart  //+.+
    {
        Unset = 0,
        Root,
        Stem,
        Leaf,
        Flower,
        Fruit,
        Seed,
    }

    [Flags]
    public enum PlantUse  //+.+
    {
        Unset = 0,
        Food = 1,
        Medicine = 2,
        Toxin = 4,
        Textile = 8,
        Flavor = 16,
        Beauty = 32,
        Magic = 64,
        Symbolism = 128,
    }
}
