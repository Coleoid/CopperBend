using SadConsole.Entities;
using Color = Microsoft.Xna.Framework.Color;

namespace CopperBend.Contract
{
    public interface IEntityInitPort
    {
        Color Foreground { get; set; }
        Color Background { get; set; }
        int Glyph { get; set; }

        IEntity SadConEntity { get; set; }
        //void SetIEntity(IEntity entity);
    }
}
