using System;
using Microsoft.Xna.Framework;
using CopperBend.Contract;
using CopperBend.Model;

namespace CopperBend.Fabric
{
    public class BeingCreator
    {
        public ISadConEntityFactory SadConEntityFactory { get; set; }

        public BeingCreator(ISadConEntityFactory sadConEntityFactory)
        {
            SadConEntityFactory = sadConEntityFactory;
        }

        public Being CreateBeing(string beingName)
        {
            if (beingName == "player")
            {
                return new Player(Guid.NewGuid(), Color.AntiqueWhite, Color.Transparent, '@')
                {
                    Name = "Suvail",
                };
            }
            else
            {
                throw new Exception($"Don't know how to CreateBeing(\"{beingName}\").");
            }
        }

        public Being CreateBeing(Color foreground, Color background, int glyph, uint id = uint.MaxValue)
        {
            var being = new Being(Guid.NewGuid(), foreground, background, glyph, id);
            being.SadConEntity = SadConEntityFactory.GetSadCon(being);
            return being;
        }
    }
}
