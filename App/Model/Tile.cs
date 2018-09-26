using System;
using RLNET;
using RogueSharp;

namespace CopperBend.App.Model
{
    public interface ITile : IDrawable, ICoord
    {
        TerrainType TerrainType { get; }

        RLColor ColorBackground { get; }
        bool IsTillable();
        bool IsTilled { get; }
        void Till();
    }

    public class Tile : ITile
    {
        public bool IsInFOV;
        internal TileRepresentation repr;
        public TerrainType TerrainType { get; private set; }

        public Tile(int x, int y, TerrainType type)
        {
            X = x;
            Y = y;
            TerrainType = type;
            repr = TileRepresenter.OfTerrain(type);
        }

        public bool IsTillable()
        {
            return TerrainType == TerrainType.Dirt;
        }

        public bool IsTilled { get; private set; }

        public void Till()
        {
            IsTilled = true;
            SetTerrainType(TerrainType.TilledDirt);
            repr.Symbol = '~';
        }

        private void SetTerrainType(TerrainType newType)
        {
            TerrainType = newType;
            repr = TileRepresenter.OfTerrain(newType);
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
            throw new Exception("Tiles don't move.  Change my mind.");
        }

        public int X { get; private set; }
        public int Y { get; private set; }
    }
}
