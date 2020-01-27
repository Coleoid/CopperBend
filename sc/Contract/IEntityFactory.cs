using SadConsole.Entities;
using Color = Microsoft.Xna.Framework.Color;

namespace CopperBend.Contract
{
    public interface IEntityFactory
    {
        Entity GetSadCon(IGetSadCon cb);
    }

    public interface IGetSadCon
    {
        Color Foreground { get; set; }
        Color Background { get; set; }
        int Glyph { get; set; }
        Entity SadConEntity { get; set; }
    }
}
