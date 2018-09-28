using System;
using System.Collections.Generic;
using System.Linq;

namespace CopperBend.App.Model
{

    public class Seed : Item
    {
        public SeedType SeedType;

        public override string Name { get => "seed"; }

        public override bool SameThingAs(IItem item)
        {
            if (item is Seed seed)
            {
                return SeedType == seed.SeedType;
            }

            return false;
        }

        public Seed(int x, int y, int quantity, SeedType type)
            : base(x, y, quantity, true)
        {
            SeedType = type;
        }
    }
}
