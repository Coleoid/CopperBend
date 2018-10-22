using CopperBend.App.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RLNET;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System;

namespace CopperBend.App
{
    //  Expected to mature into a classic flyweight with no mutable state,
    //  with one per form of terrain/tile.  Eventually purely data-driven,
    //  for now I'm building these guys in code.
    public class TileType
    {
        public string Name { get; set; }
        public char Symbol { get; set; }
        public bool IsWalkable { get; set; }
        public bool IsTransparent { get; set; }
        public bool IsTillable { get; internal set; }

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
    }


    public class MapLoader
    {
        Dictionary<string, TileType> TileTypes;

        public MapLoader()
        {
            InitTileTypes();
        }

        public void InitTileTypes()
        {
            TileTypes = new Dictionary<string, TileType>();

            var type = new TileType
            {
                Name = "Unknown",
                Symbol = '?',
                IsWalkable = true,
                IsTransparent = true,
            };
            type.SetForeground(Palette.DbBlood, Palette.DbBlood);
            type.SetBackground(Colors.FloorBackground, Colors.FloorBackgroundSeen);
            TileTypes["Unknown"] = type;

            type = new TileType
            {
                Name = "Dirt",
                Symbol = '.',
                IsWalkable = true,
                IsTransparent = true,
            };
            type.SetForeground(Colors.Floor, Colors.FloorSeen);
            type.SetBackground(Colors.FloorBackground, Colors.FloorBackgroundSeen);
            TileTypes["Dirt"] = type;

            type = new TileType
            {
                Name = "TilledDirt",
                Symbol = '~',
                IsWalkable = true,
                IsTransparent = true,
            };
            type.SetForeground(Colors.Floor, Colors.FloorSeen);
            type.SetBackground(Colors.FloorBackground, Colors.FloorBackgroundSeen);
            TileTypes["TilledDirt"] = type;

            type = new TileType
            {
                Name = "Blight",
                Symbol = ',',
                IsWalkable = true,
                IsTransparent = true,
            };
            type.SetForeground(Palette.DbOldBlood, Palette.DbBlood);
            type.SetBackground(Colors.FloorBackground, Colors.FloorBackgroundSeen);
            TileTypes["Blight"] = type;

            type = new TileType
            {
                Name = "StoneWall",
                Symbol = '#',
                IsWalkable = false,
                IsTransparent = false,
            };
            type.SetForeground(Colors.Wall, Colors.WallSeen);
            type.SetBackground(Colors.WallBackground, Colors.WallBackgroundSeen);
            TileTypes["StoneWall"] = type;

            type = new TileType
            {
                Name = "ClosedDoor",
                Symbol = '+',
                IsWalkable = false,
                IsTransparent = false,
            };
            type.SetForeground(Colors.Wall, Colors.WallSeen);
            type.SetBackground(Colors.FloorBackground, Colors.FloorBackgroundSeen);
            TileTypes["ClosedDoor"] = type;

            type = new TileType
            {
                Name = "OpenDoor",
                Symbol = '-',
                IsWalkable = true,
                IsTransparent = true,
            };
            type.SetForeground(Colors.Wall, Colors.WallSeen);
            type.SetBackground(Colors.FloorBackground, Colors.FloorBackgroundSeen);
            TileTypes["OpenDoor"] = type;

            type = new TileType
            {
                Name = "WoodenFence",
                Symbol = 'X',
                IsWalkable = false,
                IsTransparent = false,
            };
            type.SetForeground(Colors.Wall, Colors.WallSeen);
            type.SetBackground(Colors.FloorBackground, Colors.FloorBackgroundSeen);
            TileTypes["WoodenFence"] = type;

            type = new TileType
            {
                Name = "Wall",
                Symbol = '=',
                IsWalkable = false,
                IsTransparent = false,
            };
            type.SetForeground(Colors.Wall, Colors.WallSeen);
            type.SetBackground(Colors.FloorBackground, Colors.FloorBackgroundSeen);
            TileTypes["Wall"] = type;

            type = new TileType
            {
                Name = "Gate",
                Symbol = '%',
                IsWalkable = false,
                IsTransparent = false,
            };
            type.SetForeground(Colors.Wall, Colors.WallSeen);
            type.SetBackground(Colors.FloorBackground, Colors.FloorBackgroundSeen);
            TileTypes["Gate"] = type;
        }


        public IAreaMap LoadMap(string mapName)
        {
            return MapFromFile(mapName);
        }

        public IAreaMap MapFromFile(string mapName)
        {
            return null;
        }

        public IAreaMap MapFromYAML(string mapYaml)
        {
            var data = DataFromYAML(mapYaml);
            var width = data.Terrain.Max(t => t.Length);
            var height = data.Terrain.Count();
            var map = new AreaMap(width, height);

            map.Name = data.Name;

            for (int y = 0; y < height; y++)
            {
                string row = data.Terrain[y];
                for (int x = 0; x < width; x++)
                {
                    var symbol = (x < row.Length)
                        ? row.Substring(x, 1)
                        : "???";
                    var name = data.Legend[symbol];

                    var type = TerrainFrom(name);
                    map.Tiles[x, y] = new Tile(x, y, type);


                    //TODO:  push down/unify
                    map.SetCellProperties(x, y, type.IsTransparent, type.IsWalkable);
                }
            }

            return map;
        }


        internal IAreaMap FarmMap()
        {
            string FarmMapYaml = @"---
name:  Demo

legend:
 '.': Dirt
 '#': WoodenFence
 '+': ClosedDoor
 'x': TilledDirt
 '=': Wall
 '|': Gate
 '-': Gate

terrain:
#   0    5    1    5    2    5    3    5    4 
 - '#########################################'  # 0
 - '#.......................................#'
 - '#...........................xxxxxxxxxx..#'
 - '#.xxxxxxxxxxxxxx......====..xxxxxxxxxx..#'
 - '#.xxxxxxxxxxxxxx......=..=..xxxxxxxxxx..#'
 - '#.xxxxxxxxxxxxxx......=++=..xxxxxxxxxx..#'  # 5
 - '#.xxxxxxxxxxxxxx............xxxxxxxxxx..#'
 - '#.xxxxxxxxxxxxxx............xxxxxxxxxx..#'
 - '#.xxxxxxxxxxxxxx........................#'
 - '#.xxxxxxxxxxxxxx............========....#'
 - '#.xxxxxxxxxxxxxx............=......=....#'  # 10
 - '#.xxxxxxxxxxxxxx............=......=....#'
 - '#.xxxxxxxxxxxxxx............=......=....#'
 - '#.xxxxxxxxxxxxxx............=......=....#'
 - '#.xxxxxxxxxxxxxx............+......=....#'
 - '|...........................=......=....#'  # 5
 - '|...........................=......=....#'
 - '+...........................========....#'
 - '#.xxxxxxxxxxxxxx........................#'
 - '#.xxxxxxxxxxxxxx......==................#'
 - '#.xxxxxxxxxxxxxx......==................#'  # 20
 - '#.xxxxxxxxxxxxxx..........#+##########..#'
 - '#.xxxxxxxxxxxxxx..........#..........#..#'
 - '#.xxxxxxxxxxxxxx..........#..........#..#'
 - '#.xxxxxxxxxxxxxx..........#..........#..#'
 - '#.xxxxxxxxxxxxxx..#---#...#....#+#####..#'  # 5
 - '#.xxxxxxxxxxxxxx..#...|...+....#.....#..#'
 - '#.xxxxxxxxxxxxxx..#...#...############..#'
 - '#.xxxxxxxxxxxxxx..#...#.................#'
 - '#.................#####.................#'
 - '#.......................................#'  # 30
 - '#########################################'
";
            var map = MapFromYAML(FarmMapYaml);

            string BlightOverlayYaml = @"---
name:  Blight Overlay

legend:
 '.': Empty
 'o': Blight

terrain:
 - '..oooo'
 - '...oooo'
 - '....ooo'
 - '.ooooooo'
 - 'ooooooooooo...oo'
 - '.oooooooooooooooo'
 - '.oooooooooooooo'
 - 'ooooooooooooooo'
 - 'oooooooooooooo'
 - '.oooooooooo.oo'
 - '.oooooooooo.oo'
 - '.oooooooooo'
 - '.oooooooooooo'
 - '.oooooooooooo'
 - '.oooo oooooo'
 - '..o........o'
 - '..oo......oo'
 - '..oo......o'
 - '...o......ooo'
 - '...o.......ooo'
 - '...ooo'
 - '...ooo....o'
 - '...oooo..oo'
 - '....oooooo'
 - '.....ooooo'
 - '.......ooo'
 - '.......o'
";

            map.TileTypes = TileTypes;
            return map;
        }


        internal IAreaMap DemoMap()
        {
            string DemoMapYaml = @"---
name:  Demo

legend:
 '.': Dirt
 '#': StoneWall
 '+': ClosedDoor

terrain:
 - '################'
 - '#..............#'
 - '#..####..####..#'
 - '#.##..##.#..##.#'
 - '#.#....+.####..#'
 - '#.##..##.#..##.#'
 - '#..####..####..#'
 - '#..............#'
 - '################'
";
            var map = MapFromYAML(DemoMapYaml);

            var rock = new Item(5, 1)
            {
                Name = "rock",
                Color = Palette.DbOldStone,
                Symbol = '*',
            };
            map.Items.Add(rock);

            var glom = new Actor(4, 1)
            {
                Name = "glom",
                Symbol = 'g',
                Color = RLColor.Green,
            };
            map.Actors.Add(glom);

            map.FirstSightMessages.Add("I wake up.  Cold--frost on the ground, except where I was lying.");
            map.FirstSightMessages.Add("Everything hurts when I stand up.");
            map.FirstSightMessages.Add("The sky... says it's morning.  A small farmhouse to the east.");
            map.FirstSightMessages.Add("Something real wrong with the ground to the west, and the north.");

            map.LocationMessages[(1,6)] = new List<string> { "a shiversome feeling..." };

            return map;
        }

        public TileType TerrainFrom(string name)
        {
            var foundType = TileTypes.ContainsKey(name) ? name : "Unknown";
            return TileTypes[foundType];
        }

        public MapData DataFromYAML(string mapYaml)
        {
            var reader = new StringReader(mapYaml);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            return deserializer.Deserialize<MapData>(reader);
        }
    }

    public class MapData
    {
        public string Name { get; set; }
        public Dictionary<string, string> Legend { get; set; }
        public List<string> Terrain { get; set; }
    }
}
