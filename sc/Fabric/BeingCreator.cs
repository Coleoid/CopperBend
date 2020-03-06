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
            var being = beingName switch
            {
                "player" => new Player(Guid.NewGuid(), Color.LawnGreen, Color.Black, '@')
                {
                    Name = "Suvail",
                    IsPlayer = true,
                },
                "flame rat" => new Being(Guid.NewGuid(), Color.Red, Color.Black, 'r')
                {
                    Name = "flame rat",
                },
                _ => throw new Exception($"Don't know how to CreateBeing(\"{beingName}\")."),
            };

            being.SadConEntity = SadConEntityFactory.GetSadCon(being);

            return being;
        }

        public Being CreateBeing(Color foreground, Color background, int glyph, uint id = uint.MaxValue)
        {
            var being = new Being(Guid.NewGuid(), foreground, background, glyph, id);
            being.SadConEntity = SadConEntityFactory.GetSadCon(being);
            return being;
        }
    }
}
