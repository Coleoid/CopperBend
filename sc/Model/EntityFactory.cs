using System;
using Microsoft.Xna.Framework;
using SadConsole.Entities;

namespace CopperBend.Model
{
    public interface IEntityFactory
    {
        void WireCbEntity(CbEntity cb, Color foreground, Color background, int glyph);
    }

    public class EntityFactory: IEntityFactory
    {
        public EntityFactory() { }

        public void WireCbEntity(CbEntity cb, Color foreground, Color background, int glyph)
        {
            cb.ScEntity = NewSCEntity(foreground, background, glyph);
        }

        private IEntity NewSCEntity(Color foreground, Color background, int glyph)
        {
            return new Entity(foreground, background, glyph);
        }
    }
}
