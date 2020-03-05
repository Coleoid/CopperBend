using CopperBend.Contract;
using CopperBend.Model;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CopperBend.Fabric
{
    public class BeingCreator
    {
        public ISadConEntityFactory SadConEntityFactory { get; set; }

        public BeingCreator(ISadConEntityFactory sadConEntityFactory)
        {
            SadConEntityFactory = sadConEntityFactory;
        }

        public Being CreateBeing(Color foreground, Color background, int glyph, uint id = uint.MaxValue)
        {
            var being = new Being(Guid.NewGuid(), foreground, background, glyph, id);
            being.SadConEntity = SadConEntityFactory.GetSadCon(being);
            return being;
        }

    }
}
