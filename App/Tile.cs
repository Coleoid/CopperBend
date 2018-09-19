using RLNET;
using RogueSharp;

namespace CopperBend.App
{
    public class Tile : IDrawable, ICoord
    {
        public bool IsInFOV;
        internal TileRepresentation repr;
        internal TerrainType TerrainType;

        public RLColor Color
        {
            get => repr.Foreground(IsInFOV);
        }
        public RLColor ColorBackground
        {
            get => repr.Background(IsInFOV);
        }

        public char Symbol
        {
            get => repr.Symbol;
        }

        public int X { get; set; }
        public int Y { get; set; }
    }
}
