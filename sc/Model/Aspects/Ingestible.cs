using CopperBend.Contract;
using System;
using System.Collections.Generic;

namespace CopperBend.Model.Aspects
{
    public class Ingestible : Usable, IIngestible
    {
        public uint PlantID { get; set; }
        public bool IsFruit { get; set; }
        public int FoodValue { get; set; }
        public int TicksToEat { get; set; }

        public Ingestible(string verbPhrase = "eat", UseTargetFlags targets = UseTargetFlags.Self)
            : base(verbPhrase, targets)
        { }
    }

    //0.+:  At some point we'll want creatures with specific appetites,
    // and players able to engage in challenge Conducts.
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
