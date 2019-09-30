using SadConsole.Entities;
using Color = Microsoft.Xna.Framework.Color;

namespace CopperBend.Contract
{
    public interface IEntityFactory
    {
        IEntity WireCbEntity(ITakeScEntity cb);
    }

    public interface ITakeScEntity
    {
        Color Foreground { get; set; }
        Color Background { get; set; }
        int Glyph { get; set; }
        IEntity ScEntity { get; set; }
    }
}
