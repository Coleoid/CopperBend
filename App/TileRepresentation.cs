using System;
using System.Collections.Generic;
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
            if (ReprOfTerrain == null)
                InitRepresentationsOfTerrain();

            if (ReprOfTerrain.ContainsKey(terrain))
                return ReprOfTerrain[terrain];

            throw new Exception($"No represenation coded for terrain type [{terrain}].");
        }
        public static Dictionary<TerrainType, TileRepresentation> ReprOfTerrain;

        private static void InitRepresentationsOfTerrain()
        {
            ReprOfTerrain = new Dictionary<TerrainType, TileRepresentation>();

            var rep = new TileRepresentation
            {
                Symbol = '?',
            };
            rep.SetForeground(Palette.DbBlood, Palette.DbBlood);
            rep.SetBackground(Colors.FloorBackground, Colors.FloorBackgroundSeen);
            ReprOfTerrain[TerrainType.Unknown] = rep;

            rep = new TileRepresentation
            {
                Symbol = '.',
            };
            rep.SetForeground(Colors.Floor, Colors.FloorSeen);
            rep.SetBackground(Colors.FloorBackground, Colors.FloorBackgroundSeen);
            ReprOfTerrain[TerrainType.Dirt] = rep;

            rep = new TileRepresentation
            {
                Symbol = ',',
            };
            rep.SetForeground(Palette.DbOldBlood, Palette.DbBlood);
            rep.SetBackground(Colors.FloorBackground, Colors.FloorBackgroundSeen);
            ReprOfTerrain[TerrainType.Blight] = rep;

            rep = new TileRepresentation
            {
                Symbol = '#',
            };
            rep.SetForeground(Colors.Wall, Colors.WallSeen);
            rep.SetBackground(Colors.WallBackground, Colors.WallBackgroundSeen);
            ReprOfTerrain[TerrainType.StoneWall] = rep;

            rep = new TileRepresentation
            {
                Symbol = '+',
            };
            rep.SetForeground(Colors.Wall, Colors.WallSeen);
            rep.SetBackground(Colors.FloorBackground, Colors.FloorBackgroundSeen);
            ReprOfTerrain[TerrainType.Door] = rep;
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
