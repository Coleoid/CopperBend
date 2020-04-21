using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Color = Microsoft.Xna.Framework.Color;
using SadConsole;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;

namespace CopperBend.Persist
{
    public class MapLoader
    {
        private readonly ILog log;
        public Atlas Atlas { get; }

        public MapLoader(ILog logger)
        {
            log = logger;
            Atlas = new Atlas();
        }

        public CompoundMap LoadDevMap(string mapName, GameState state)
        {
            CompoundMap map;
            if (mapName == "Farm")
                map = FarmMap();
            else if (mapName == "Farmhouse")
                map = FarmhouseMap();
            else
                map = DemoMap();

            return map;
        }

        public CompoundMap MapFromYAML(string mapYaml)
        {
            MapData data = DataFromYAML(mapYaml);
            var width = data.Terrain.Max(t => t.Length);
            var height = data.Terrain.Count;

            var map = new CompoundMap
            {
                Width = width,
                Height = height,
                SpaceMap = new SpaceMap(width, height),
                BeingMap = new MultiSpatialMap<IBeing>(),
                ItemMap = new ItemMap(),
                RotMap = new RotMap(),
            };

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
                    Space space = new Space
                    {
                        Terrain = type,
                    };
                    map.SpaceMap.Add(space, (x, y));

                    if (type == SpaceMap.TilledSoil) map.SpaceMap.Till(space);
                }
            }

            foreach (var overlay in data.Rot ?? new List<RotOverlayData>())
            {
                var nums = Regex.Split(overlay.Location, ",");
                int x_off = int.Parse(nums[0], CultureInfo.InvariantCulture);
                int y_off = int.Parse(nums[1], CultureInfo.InvariantCulture);
                var w = overlay.Terrain.Max(t => t.Length);
                var h = overlay.Terrain.Count;
                for (int y = 0; y < h; y++)
                {
                    var row = overlay.Terrain[y].ToCharArray();
                    for (int x = 0; x < w; x++)
                    {
                        char symbol = (x < row.Length) ? row[x] : '.';

                        bool isD = '0' <= symbol && symbol <= '9';
                        int extent = isD ? symbol - '0' : 0;
                        if (extent > 0)
                            map.RotMap.Add(new AreaRot { Health = extent }, (x + x_off, y + y_off));
                    }
                }
            }

            return map;
        }

        //0.1: Map loading is so hard-codey
        private CompoundMap farmMap = null;
        internal CompoundMap FarmMap()
        {
            if (farmMap == null)
            {
                farmMap = MapFromYAML(farmMapYaml);
                farmMap.SpaceMap.PlayerStartPoint = (23, 21);  //0.1.MAP  get start location from map
                Coord shedCoord = (28, 4);
                var gear = Equipper.BuildItem("hoe");
                farmMap.ItemMap.Add(gear, shedCoord);
                gear = Equipper.BuildItem("gloves");
                farmMap.ItemMap.Add(gear, shedCoord);

                //  Obscure point on the edge to test map transitions
                //_farmMap.AddEventAtLocation(new Point(41, 1), new CommandEntry(GameCommand.GoToFarmhouse, null));

                ////  Barrier in front of the gate out
                //_farmMap.AddEventAtLocation(new Point(5, 16), new CommandEntry(GameCommand.NotReadyToLeave, null));
                //_farmMap.AddEventAtLocation(new Point(5, 17), new CommandEntry(GameCommand.NotReadyToLeave, null));
                //_farmMap.AddEventAtLocation(new Point(5, 18), new CommandEntry(GameCommand.NotReadyToLeave, null));
            }

            log.Debug("Loaded the farmyard map");
            return farmMap;
        }

        private CompoundMap farmhouseMap = null;
        internal CompoundMap FarmhouseMap()
        {
            if (farmhouseMap == null)
            {
                farmhouseMap = MapFromYAML(rootCellarYaml);
                //_farmhouseMap.PlayerStartsAt = new Point(2, 7);
            }

            return farmhouseMap;
        }

        public Terrain TerrainFrom(string name)
        {
            name = name.ToLowerInvariant();
            var foundType = Atlas.Legend.ContainsKey(name) ? name : "Unknown";
            return Atlas.Legend[foundType];
        }

        public MapData DataFromYAML(string mapYaml)
        {
            using var reader = new StringReader(mapYaml);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var mapData = deserializer.Deserialize<MapData>(reader);
            return mapData;
        }

        internal CompoundMap DemoMap()
        {
            string demoMapYaml = @"---
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
            var map = MapFromYAML(demoMapYaml);

            //var rock = new Item(new Point(5, 1))
            //{
            //    Name = "rock",
            //    ColorForeground = Color.DimGray,
            //    Symbol = '*',
            //};
            //map.Items.Add(rock);

            //var glom = new Monster(Color.Green, Color.Transparent, 'g')
            //{
            //    Name = "glom",
            //};
            //map.Actors.Add(glom);

            //map.LocationMessages[new Point(1, 6)] = new List<string> { "a shiversome feeling..." };

            return map;
        }


        #region Farm and farmhouse map YAML
        private readonly string farmMapYaml = @"---
name:  Farm

legend:
 '.': soil
 '#': wooden fence
 '+': closed door
 'x': tilled soil
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

rot:
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


        private readonly string rootCellarYaml = @"---
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
}
