using RLNET;
using RogueSharp;

namespace CopperBend.App
{
    public interface ITile : IDrawable, ICoord
    {
        TileType TileType { get; }

        RLColor ColorBackground { get; }
        bool IsTillable { get; }
        bool IsTilled { get; }
        bool IsSown { get; }
        void Till();
        void Sow(ISeed seed);

        void RemovePlant();
        void SetTileType(TileType tileType);
    }
}
