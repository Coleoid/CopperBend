using System;
using System.Collections.Generic;
using RLNET;

namespace CopperBend.App
{
    public class TileRepresentation
    {
        internal void SetForeground(RLColor colorUnseen, RLColor colorSeen)
        {
            _fgUnseen = colorUnseen;
            _fgSeen = colorSeen;
        }
        public RLColor Foreground(bool isInFOV) => isInFOV ? _fgSeen : _fgUnseen;
        private RLColor _fgSeen;
        private RLColor _fgUnseen;

        internal void SetBackground(RLColor colorUnseen, RLColor colorSeen)
        {
            _bgUnseen = colorUnseen;
            _bgSeen = colorSeen;
        }
        public RLColor Background(bool isInFOV) => isInFOV ? _bgSeen : _bgUnseen;
        private RLColor _bgSeen;
        private RLColor _bgUnseen;

        public char Symbol { get; set; }
    }

    public class TileRepresenter
    {
        public static TileRepresentation OfTerrain(TerrainType terrain)
        {
            if (ReprOfTerrain == null)
                InitRepresentationsOfTerrain();

            if (ReprOfTerrain.ContainsKey(terrain))
                return ReprOfTerrain[terrain];

            throw new Exception($"No represenation coded for terrain type [{terrain}].");
        }
        private static Dictionary<TerrainType, TileRepresentation> ReprOfTerrain;

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
    }
}
