using CopperBend.Model;
using Microsoft.Xna.Framework;

namespace CopperBend.Contract
{
    public interface ITile : IDrawable
    {
        Point Point { get; }
        TileType TileType { get; }
        void SetTileType(TileType tileType);
        //  Doesn't go in IDrawable, only tiles have backgrounds
        Color ColorBackground { get; }
        
        bool IsTillable { get; }
        bool IsTilled { get; }
        bool IsSown { get; }
        void Till();
        void Sow(ISeed seed);
        void RemovePlant();

        int BlightLevel { get; set; }
        bool IsInFOV { get; set; }
    }
}
