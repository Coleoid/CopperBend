using System;
using Microsoft.Xna.Framework;
using SadConsole.Entities;

namespace CopperBend.Model
{
    public class ScEntityFactory
    {
        public static bool ReturnNull = false;
        public ScEntityFactory() { }

        internal Entity NewEntity(Color foreground, Color background, int glyph)
        {
            if (ScEntityFactory.ReturnNull) return null;
            return new Entity(foreground, background, glyph);
        }
    }
}
