using System;
using RLNET;

namespace CopperBend.App
{
    public class TileRepresentation
    {
        public bool IsInFOV;

        public RLColor Foreground { get => IsInFOV? _fgFOV : _fg; }
        private RLColor _fg;
        private RLColor _fgFOV;

        public RLColor Background { get => IsInFOV ? _bgFOV : _bg; }
        private RLColor _bg;
        private RLColor _bgFOV;

        public char Symbol { get; set; }

        public static TileRepresentation OfTerrain(TerrainType terrain)
        {
            switch (terrain)
            {
                case TerrainType.Unknown: return ReprUnknown;
                case TerrainType.Dirt: return ReprFloor;
                case TerrainType.Blight: return ReprBlight;
                case TerrainType.StoneWall: return ReprWall;

                default:
                    throw new Exception($"No represenation coded for terrain type [{terrain}].");
            }
        }

        public static TileRepresentation ReprUnknown;
        public static TileRepresentation ReprFloor;
        public static TileRepresentation ReprWall;
        public static TileRepresentation ReprBlight;

        static TileRepresentation()
        {
            ReprUnknown = new TileRepresentation
            {
                Symbol = '?',
            };
            ReprUnknown.SetForeground(Palette.DbBlood, Palette.DbBlood);
            ReprUnknown.SetBackground(Colors.FloorBackground, Colors.FloorBackgroundFov);

            ReprFloor = new TileRepresentation
            {
                Symbol = '.',
            };
            ReprFloor.SetForeground(Colors.Floor, Colors.FloorFov);
            ReprFloor.SetBackground(Colors.FloorBackground, Colors.FloorBackgroundFov);

            ReprBlight = new TileRepresentation
            {
                Symbol = ',',
            };
            ReprBlight.SetForeground(Palette.DbOldBlood, Palette.DbBlood);
            ReprBlight.SetBackground(Colors.FloorBackground, Colors.FloorBackgroundFov);

            ReprWall = new TileRepresentation
            {
                Symbol = '#',
            };
            ReprWall.SetForeground(Colors.Wall, Colors.WallFov);
            ReprWall.SetBackground(Colors.WallBackground, Colors.WallBackgroundFov);
        }

        private void SetForeground(RLColor color, RLColor colorInFOV)
        {
            _fg = color;
            _fgFOV = colorInFOV;
        }

        private void SetBackground(RLColor color, RLColor colorInFOV)
        {
            _bg = color;
            _bgFOV = colorInFOV;
        }
    }

    
}
