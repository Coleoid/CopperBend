using Microsoft.Xna.Framework;
using SadConsole.Entities;
using CopperBend.Contract;

namespace CopperBend.Engine
{
    public class EntityFactory: IEntityFactory
    {
        public EntityFactory() { }

        public void WireCbEntity(ITakeScEntity cb, Color foreground, Color background, int glyph)
        {
            cb.ScEntity = NewSCEntity(foreground, background, glyph);
        }

        private IEntity NewSCEntity(Color foreground, Color background, int glyph)
        {
            return new Entity(foreground, background, glyph);
        }
    }
}
