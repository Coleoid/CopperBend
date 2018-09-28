using System;
using RLNET;

namespace CopperBend.App.Model
{
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

        #region Cultivation

        public bool IsTillable => TerrainType == TerrainType.Dirt;

        public bool IsTilled => TerrainType == TerrainType.TilledDirt;

        public void Till()
        {
            SetTerrainType(TerrainType.TilledDirt);
        }

        public IItem SownSeed { get; private set; }
        public void Sow(IItem seed)
        {
            Guard.Against(!(seed is Seed));
            SownSeed = seed;
        }

        #endregion

        public void OpenDoor()
        {
            Guard.Against(TerrainType != TerrainType.ClosedDoor);
            SetTerrainType(TerrainType.OpenDoor);
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
