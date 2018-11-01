using System;
using RLNET;
using RogueSharp;

namespace CopperBend.App.Model
{
    public class Tile : ITile
    {
        public Tile(int x, int y, TileType type)
        {
            Point = new Point(x, y);
            TileType = type;
        }

        public Point Point { get; }

        public TileType TileType { get; private set; }
        public char Symbol => TileType.Symbol;
        public void SetTileType(TileType newType)
        {
            TileType = newType;
        }

        public RLColor ColorForeground => TileType.Foreground(IsInFOV);
        public RLColor ColorBackground
        {
            get
            {
                RLColor mauve = new RLColor(.878f, .69f, 1f);
                RLColor bg = TileType.Background(IsInFOV);
                bg = RLColor.Blend(mauve, bg, BlightLevel / 7f);
                return bg;
            }
        }


        public bool IsInFOV { get; set; }
        public int BlightLevel { get; set; }

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

        public void RemovePlant()
        {
            SownSeed = null;
        }

        #endregion
    }
}
