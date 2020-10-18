using SadConsole.Entities;
using CopperBend.Contract;
using SadConsole;

namespace CopperBend.Fabric
{
    public class SadConEntityFactory : ISadConEntityFactory
    {
        private FontMaster MapFontMaster { get; set; }
        public Font MapFont { get; set; }

        public SadConEntityFactory(FontMaster mapFontMaster)
        {
            MapFontMaster = mapFontMaster;
            MapFont = MapFontMaster.GetFont(Font.FontSizes.One);
        }

        public void SetIEntityOnPort(IEntityInitPort init)
        {
            if (init.SadConEntity == null)
                init.SadConEntity = GetIEntity(init);
        }

        private IEntity GetIEntity(IEntityInitPort init)
        {
            return new Entity(init.Foreground, init.Background, init.Glyph) { Font = MapFont };
        }
    }
}
