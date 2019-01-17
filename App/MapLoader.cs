using CopperBend.App.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RLNET;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Text.RegularExpressions;
using CopperBend.MapUtil;
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
        private RLColor _bgSeen = Colors.FloorBackgroundSeen;
        private RLColor _bgUnseen = Colors.FloorBackground;
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
            StoreTileType(type);

            type = new TileType
            {
                Name = "dirt",
                Symbol = '.',
                IsWalkable = true,
                IsTransparent = true,
                IsTillable = true,
            };
            type.SetForeground(Colors.Floor, Colors.FloorSeen);
            StoreTileType(type);

            type = new TileType
            {
                Name = "tilled dirt",
                Symbol = '~',
                IsWalkable = true,
                IsTransparent = true,
                IsTillable = true,
                
            };
            type.SetForeground(Palette.DbWood, Palette.DbBrightWood);
            StoreTileType(type);


            type = new TileType
            {
                Name = "stone wall",
                Symbol = '#',
                IsWalkable = false,
                IsTransparent = false,
            };
            type.SetForeground(Colors.Wall, Colors.WallSeen);
            type.SetBackground(Colors.WallBackground, Colors.WallBackgroundSeen);
            StoreTileType(type);

            type = new TileType
            {
                Name = "closed door",
                Symbol = '+',
                IsWalkable = false,
                IsTransparent = false,
            };
            type.SetForeground(Colors.Wall, Colors.WallSeen);
            StoreTileType(type);

            type = new TileType
            {
                Name = "open door",
                Symbol = '-',
                IsWalkable = true,
                IsTransparent = true,
            };
            type.SetForeground(Colors.Wall, Colors.WallSeen);
            StoreTileType(type);

            type = new TileType
            {
                Name = "wooden fence",
                Symbol = 'X',
                IsWalkable = false,
                IsTransparent = false,
            };
            type.SetForeground(Palette.DbWood, Palette.DbBrightWood);
            StoreTileType(type);

            type = new TileType
            {
                Name = "wall",
                Symbol = '=',
                IsWalkable = false,
                IsTransparent = false,
            };
            type.SetForeground(Colors.Wall, Colors.WallSeen);
            StoreTileType(type);

            type = new TileType
            {
                Name = "gate",
                Symbol = '%',
                IsWalkable = false,
                IsTransparent = true,
            };
            type.SetForeground(Colors.Wall, Colors.WallSeen);
            StoreTileType(type);

            type = new TileType
            {
                Name = "grass",
                Symbol = ',',
                IsWalkable = true,
                IsTransparent = true,
                IsTillable = true,
            };
            type.SetForeground(Palette.DbVegetation, Palette.DbGrass);
            StoreTileType(type);

            type = new TileType
            {
                Name = "tall weeds",
                Symbol = 'w',
                IsWalkable = true,
                IsTransparent = true,
            };
            type.SetForeground(Palette.DbVegetation, Palette.DbGrass);
            StoreTileType(type);

            type = new TileType
            {
                Name = "table",
                Symbol = 'T',
                IsWalkable = false,
                IsTransparent = true,
            };
            type.SetForeground(Palette.DbWood, Palette.DbBrightWood);
            StoreTileType(type);

            type = new TileType
            {
                Name = "stairs",
                Symbol = '>',
                IsWalkable = true,
                IsTransparent = true,
            };
            type.SetForeground(Palette.Alternate, Palette.AlternateLighter);
            StoreTileType(type);
        }

        public void StoreTileType(TileType type)
        {
            if (TileTypes.ContainsKey(type.Name))
                throw new Exception($"Already have type {type.Name} stored.");

            TileTypes[type.Name] = type;
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

            var map = new AreaMap(width, height)
            {
                Name = data.Name
            };

            var tilledType = TileTypes["tilled dirt"];  // larva

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
                    Tile tile = new Tile(x, y, type);
                    if (type == tilledType) tile.Till();
                    map.Tiles[x, y] = tile;

                    //TODO:  push down/unify
                    
                    map.SetTransparent(tile.Point, type.IsTransparent);
                    map.SetWalkable(tile.Point, type.IsWalkable);
                }
            }

            foreach (var overlay in data.Blight ?? new List<BlightOverlayData>())
            {
                var nums = Regex.Split(overlay.Location, ",");
                int x_off = int.Parse(nums[0]);
                int y_off = int.Parse(nums[1]);
                var w = overlay.Terrain.Max(t => t.Length);
                var h = overlay.Terrain.Count();
                for (int y = 0; y < h; y++)
                {
                    string row = overlay.Terrain[y];
                    for (int x = 0; x < w; x++)
                    {
                        var symbol = (x < row.Length)
                            ? row.Substring(x, 1)
                            : ".";

                        bool isD = symbol.CompareTo("0") > -1 && symbol.CompareTo("9") < 1;
                        int level = isD? int.Parse(symbol) : 0;

                        map.Tiles[x + x_off, y + y_off].BlightLevel = level;
                    }
                }
            }

            map.TileTypes = TileTypes;

            map.FirstSightMessage = data.FirstSightMessage;

            return map;
        }

        private IAreaMap _farmMap = null;
        internal IAreaMap FarmMap()
        {
            if (_farmMap == null)
            {
                _farmMap = MapFromYAML(FarmMapYaml);
                _farmMap.PlayerStartsAt = new Point(23, 21);

                //  Obscure point on the edge to test map transitions
                _farmMap.AddEventAtLocation(new Point(41, 1), new CommandEntry(GameCommand.GoToFarmhouse, null));

                //  Barrier in front of the gate out
                _farmMap.AddEventAtLocation(new Point(5, 16), new CommandEntry(GameCommand.NotReadyToLeave, null));
                _farmMap.AddEventAtLocation(new Point(5, 17), new CommandEntry(GameCommand.NotReadyToLeave, null));
                _farmMap.AddEventAtLocation(new Point(5, 18), new CommandEntry(GameCommand.NotReadyToLeave, null));
            }

            return _farmMap;
        }

        private IAreaMap _farmhouseMap = null;
        internal IAreaMap FarmhouseMap()
        {
            if (_farmhouseMap == null)
            {
                _farmhouseMap = MapFromYAML(RootCellarYaml);
                _farmhouseMap.PlayerStartsAt = new Point(2, 7);
            }

            return _farmhouseMap;
        }

        public TileType TerrainFrom(string name)
        {
            name = name.ToLower();
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


        internal IAreaMap DemoMap()
        {
            string DemoMapYaml = @"---
name:  Demo

legend:
 '.': dirt
 '#': stone wall
 '+': closed door

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

            var rock = new Item(new Point(5, 1))
            {
                Name = "rock",
                ColorForeground = Palette.DbOldStone,
                Symbol = '*',
            };
            map.Items.Add(rock);

            var glom = new Actor(new Point(4, 1))
            {
                Name = "glom",
                Symbol = 'g',
                ColorForeground = RLColor.Green,
            };
            map.Actors.Add(glom);

            map.LocationMessages[new Point(1, 6)] = new List<string> { "a shiversome feeling..." };

            return map;
        }


        #region Farm and farmhouse map text
        private readonly string FarmMapYaml = @"---
name:  Farm

legend:
 '.': dirt
 '#': wooden fence
 '+': closed door
 'x': tilled dirt
 '=': wall
 '|': gate
 '-': gate
 ',': grass
 'w': tall weeds
 'T': table
 '>': stairs down

firstSightMessage:
 - 'I wake up.  Cold--frost on the ground, except where I was lying.'
 - 'Everything hurts when I stand up.'
 - 'The sky... says it''s morning.  A small farmhouse to the east.'
 - 'Something real wrong with the ground to the west, and the northwest.'

terrain:
#   0    +    1    +    2    +    3    +    4    +
 - ',,,,,#########################################'  # 0
 - ',,,,,#,,,....,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,#'
 - ',,,,,#,,,,...,,,,,,,,,,,,,,,,,,,,x.x.xxx..x,,#'
 - ',,,,,#.xx...xxx...xxx.,,,,.====,,x..xx.xxx.,,#'
 - ',,,,,#...xxx.xx.x.xx..,,,,.=..=,,xxxx.x..xx,,#'
 - ',,,,,#.x....x..xxxxx...,,,,=++=,,x.x.xxxx.x,,#'  # 5
 - ',,,,,#.xxx.xxxx...xxx.,,,,,,..,,,..xxx..xxx,,#'
 - ',,,,,#.xx.xx.xxx.xx.x.,,......,,,x.x.x.x.xx,,#'
 - ',,,,,#.xx....xxx..xxx.....,,,,,,,,,,,,,,,,,,,#'
 - ',,,,,#.xxxxx.x.xx.xxx.,,,,,,,,,,,==========,,#'
 - ',,,,,#.xx..xxx.xxxx.x.,,,,,,,,,,,=.+.=....=,,#'  # 10
 - ',,,,,#.xxxxx.xx...x.x.,,,,,,,,,,,=.=.+....=,,#'
 - ',,,,,#.xx....xx..xxxx.,,,,,,,,,,,===.======,,#'
 - ',,,,,#.xx.xxx..xxxx...,,,,.......=.......>=,,#'
 - ',,,,,#.xxx..xx....xxx.,..........+........=,,#'
 - '.....|......................,,,,.=..TT...T=,,#'  # 5
 - '.....|.....................,,,,,.=.....TTT=,,#'
 - '.....+................,,,...,,,,.==========,,#'
 - ',,,,,#.xxx..xxxxx.xxx.,,,,..,,..,,,,,,,,,,,,,#'
 - ',,,,,#.xx.xxx..xxxx.x......==..,,,,,,,,,,,,,,#'
 - ',,,,,#.xxx...xxx..xxx.,,,,.==....,,,,,,,,,,,,#'  # 20
 - ',,,,,#...xx...xx.xxxx.,,,,,,,,,#+##########,,#'
 - ',,,,,#.xx...xxxxxx.xx.,,,,,,,,,#..........#,,#'
 - ',,,,,#.x.xxx..xxxx..x.,,,,,,,,,#..........#,,#'
 - ',,,,,#.xxx..x..xxxx...,,,ww,,,,#..........#,,#'
 - ',,,,,#.x..xxx.xxxx.xx.,#---#,,,#..........#,,#'  # 5
 - ',,,,,#...xx..xxx..xxx.,#,,,|,..+....#+#####,,#'
 - ',,,,,#.xxx..xxx.xxx.x.,#,,,#w,,#....#.....#,,#'
 - ',,,,,#.xxxxxx.xx..xx..,#,,,#w,,#,,,,#.....#,,#'
 - ',,,,,#................,#####ww,############,,#'
 - ',,,,,#,,,,,,,,,,,,,,,,,,,,wwww,,,,,,,,,,,,,,,#'  # 30
 - ',,,,,#########################################'

blight:
  - name: one
    location: 6,0
    terrain:
      - '..1221'
      - '...1211'
      - '....121'
      - '.1112211'
      - '11122221111...11'
      - '.1111232221111111'
      - '.11113332221111'
      - '111111333221111'
      - '11111333221111'
      - '.1111333111.11'
      - '.1133221111.11'
      - '.1123232111'
      - '.112111331111'
      - '.112111122111'
      - '.1111 111111'
      - '..1........1'
      - '..11......11'
      - '..11......1'
      - '...1......111'
      - '...1.......111'
      - '...111'
      - '...111....1'
      - '...1111..11'
      - '....111111'
      - '.....11111'
      - '.......111'
      - '.......1'
";


        private readonly string RootCellarYaml = @"---
name:  Root Cellar

legend:
 '.': dirt floor
 '=': plank wall
 '<': stairs up

terrain:
#   0    5    1    5    2    5    3    5    4 
 - '============'  # 0
 - '=..........='
 - '=..........='
 - '=.........<='
 - '=..........='
 - '=..........='
 - '=..........='
 - '=..........='
 - '=..........='
 - '=..........='
 - '=..........='
 - '=..........='
";
        #endregion

    }

    public class MapData
    {
        public string Name { get; set; }
        public Dictionary<string, string> Legend { get; set; }
        public List<string> Terrain { get; set; }
        public List<BlightOverlayData> Blight { get; set; }
        public List<string> FirstSightMessage { get; set; }
    }

    public class BlightOverlayData
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public List<string> Terrain { get; set; }
    }
}
