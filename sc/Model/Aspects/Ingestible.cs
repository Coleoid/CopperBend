using CopperBend.Contract;
using System;

namespace CopperBend.Model.Aspects
{
    public class Ingestible : IIngestible
    {
        public bool IsFruit { get; set; }
        public int FoodValue { get; set; }
        public int TicksToEat { get; set; }
        public (string Name, int Degree) Effect { get; set; }
        public uint PlantID { get; set; }
        public string ConsumeVerb { get; set; } = "eat";
    }

    [Flags]
    public enum FoodCategoryFlags
    {
        Unset = 0,
        Pasture = 1,
        Vegetable = 2,
        Fruit = 4,
        Honey = 8,
        Egg = 16,
        Flesh = 32,
        Organs = 64,
        Carrion = 128
    }
}
