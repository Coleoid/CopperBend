using System;
using RLNET;

namespace CopperBend.App.Model
{
    public class Tile : ITile
    {
        public bool IsInFOV;
        public TileType TileType { get; private set; }

        public Tile(int x, int y, TileType type)
        {
            X = x;
            Y = y;
            TileType = type;
        }

        #region Cultivation

        public bool IsTillable => TileType.IsTillable;

        public bool IsTilled { get; set; }

        public bool IsSown => SownSeed != null;

        public void Till()
        {
            Guard.Against(!IsTillable);
            IsTilled = true;
        }

        public ISeed SownSeed { get; private set; }
        public void Sow(ISeed seed)
        {
            SownSeed = seed;
        }

        #endregion

        public void SetTileType(TileType newType)
        {
            TileType = newType;
        }

        public RLColor Color
        {
            get => TileType.Foreground(IsInFOV);
        }
        public RLColor ColorBackground
        {
            get => TileType.Background(IsInFOV);
        }

        public char Symbol
        {
            get => TileType.Symbol;
        }

        public void MoveTo(int x, int y)
        {
            throw new Exception("Tiles don't move.  Change my mind.");
        }

        public void RemovePlant()
        {
            SownSeed = null;
        }

        public int X { get; private set; }
        public int Y { get; private set; }
    }
}
