using Microsoft.Xna.Framework;
using SadConsole.Entities;
using CopperBend.Contract;
using SadConsole;

namespace CopperBend.Engine
{
    public class EntityFactory: IEntityFactory
    {
        FontMaster MapFontMaster { get; set; }
        public Font MapFont { get; set; }

        public EntityFactory(FontMaster mapFontMaster) 
        {
            MapFontMaster = mapFontMaster;
            MapFont = MapFontMaster.GetFont(Font.FontSizes.One);
        }

        public IEntity GetSadCon(IGetSadCon cb)
        {
            return NewSCEntity(cb.Foreground, cb.Background, cb.Glyph);
        }

        private IEntity NewSCEntity(Color foreground, Color background, int glyph)
        {
            return new Entity(foreground, background, glyph) { Font = MapFont };
        }
    }
}
