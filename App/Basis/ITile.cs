using RLNET;
using RogueSharp;

namespace CopperBend.App
{
    public interface ITile : IDrawable, ICoord
    {
        TerrainType TerrainType { get; }

        RLColor ColorBackground { get; }
        bool IsTillable { get; }
        bool IsTilled { get; }
        void Till();
        void OpenDoor();
    }
}
