using CopperBend.Contract;
using System;
using System.Collections.Generic;

namespace CopperBend.Model.Aspects
{
    public class Ingestible : Usable, IIngestible
    {
        public uint PlantID { get; set; }
        public bool IsFruit { get; set; }

        public Ingestible(string verbPhrase = "eat", int timeToIngest = 8, int foodValue = 0)
            : base(verbPhrase, UseTargetFlags.Self)
        {
            AddCosts(("time", timeToIngest), ("this", 1));
            if (foodValue > 0)
                AddEffect("food", foodValue);
        }
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
