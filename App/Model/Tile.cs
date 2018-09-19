using RLNET;
using RogueSharp;

namespace CopperBend.App.Model
{

    public interface ITile : IDrawable, ICoord
    {
        RLColor ColorBackground { get; }
        bool IsTillable();
    }

    public class Tile : ITile
    {
        public bool IsInFOV;
        internal TileRepresentation repr;
        internal TerrainType TerrainType;

        public Tile(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool IsTillable()
        {
            return TerrainType == TerrainType.Dirt;
        }

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

        public void MoveTo(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; private set; }
        public int Y { get; private set; }
    }
}
