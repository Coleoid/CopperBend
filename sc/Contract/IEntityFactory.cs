using SadConsole.Entities;
using Color = Microsoft.Xna.Framework.Color;

namespace CopperBend.Contract
{
    public interface IEntityFactory
    {
        void WireCbEntity(ITakeScEntity cb, Color foreground, Color background, int glyph);
    }

    public interface ITakeScEntity
    {
        IEntity ScEntity { get; set; }
    }
}
