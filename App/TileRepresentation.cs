using System;
using RLNET;

namespace CopperBend.App
{
    public class TileRepresentation
    {
        public bool IsInFOV;

        public RLColor Foreground { get => IsInFOV? _fgSeen : _fg; }
        private RLColor _fg;
        private RLColor _fgSeen;

        public RLColor Background { get => IsInFOV ? _bgSeen : _bg; }
        private RLColor _bg;
        private RLColor _bgSeen;

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
            ReprUnknown.SetBackground(Colors.FloorBackground, Colors.FloorBackgroundSeen);

            ReprFloor = new TileRepresentation
            {
                Symbol = '.',
            };
            ReprFloor.SetForeground(Colors.Floor, Colors.FloorSeen);
            ReprFloor.SetBackground(Colors.FloorBackground, Colors.FloorBackgroundSeen);

            ReprBlight = new TileRepresentation
            {
                Symbol = ',',
            };
            ReprBlight.SetForeground(Palette.DbOldBlood, Palette.DbBlood);
            ReprBlight.SetBackground(Colors.FloorBackground, Colors.FloorBackgroundSeen);

            ReprWall = new TileRepresentation
            {
                Symbol = '#',
            };
            ReprWall.SetForeground(Colors.Wall, Colors.WallSeen);
            ReprWall.SetBackground(Colors.WallBackground, Colors.WallBackgroundSeen);
        }

        private void SetForeground(RLColor color, RLColor colorSeen)
        {
            _fg = color;
            _fgSeen = colorSeen;
        }

        private void SetBackground(RLColor color, RLColor colorSeen)
        {
            _bg = color;
            _bgSeen = colorSeen;
        }
    }

    
}
