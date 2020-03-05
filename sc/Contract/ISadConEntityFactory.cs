using SadConsole.Entities;
using Color = Microsoft.Xna.Framework.Color;

namespace CopperBend.Contract
{
    public interface ISadConEntityFactory
    {
        IEntity GetSadCon(ISadConInitData cb);
    }

    public interface ISadConInitData
    {
        Color Foreground { get; set; }
        Color Background { get; set; }
        int Glyph { get; set; }
        IEntity SadConEntity { get; set; }
    }
}
