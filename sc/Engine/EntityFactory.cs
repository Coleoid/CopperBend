using Microsoft.Xna.Framework;
using SadConsole.Entities;
using CopperBend.Contract;

namespace CopperBend.Engine
{
    public class EntityFactory: IEntityFactory
    {
        public EntityFactory() { }

        public IEntity WireCbEntity(ITakeScEntity cb)
        {
            return NewSCEntity(cb.Foreground, cb.Background, cb.Glyph);
        }

        private IEntity NewSCEntity(Color foreground, Color background, int glyph)
        {
            return new Entity(foreground, background, glyph);
        }
    }
}
