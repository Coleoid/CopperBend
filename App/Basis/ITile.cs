using RLNET;
using RogueSharp;

namespace CopperBend.App
{
    public interface ITile : IDrawable
    {
        Point Point { get; }
        TileType TileType { get; }
        void SetTileType(TileType tileType);
        //  Doesn't go in IDrawable, only tiles have backgrounds
        RLColor ColorBackground { get; }
        
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
