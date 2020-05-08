using System.IO;
using System.Linq;
using log4net;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;
using Microsoft.Xna.Framework;
using System.Text;
using System.Collections.Generic;

namespace CopperBend.Persist
{
    public class MapLoader
    {
        private readonly ILog log;
        public Atlas Atlas { get; }

        public MapLoader(ILog logger, Atlas atlas)
        {
            log = logger;
            Atlas = atlas;
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
            CompoundMapData data = DataFromYAML(mapYaml);
            CompoundMap map = MapFromData(data);
            return map;
        }

        public CompoundMapData DataFromYAML(string mapYaml)
        {
            using var reader = new StringReader(mapYaml);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new YConv_Coord())
                .Build();

            var mapData = deserializer.Deserialize<CompoundMapData>(reader);
            return mapData;
        }

        public string YAMLFromData(CompoundMapData data)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new YConv_Coord())
                .Build();

            var yaml = serializer.Serialize(data);
            return yaml;
        }

        public CompoundMap MapFromData(CompoundMapData data)
        {
            var width = data.Terrain.Max(t => t.Length);
            var height = data.Terrain.Count;
            if (data.Width != width)
                log.Error($"Map {data.Name} declares width {data.Width} but has terrain {width} wide.");
            if (data.Height != height)
                log.Error($"Map {data.Name} declares height {data.Height} but has terrain {height} high.");

            var map = new CompoundMap
            {
                Name = data.Name,
                Width = width,
                Height = height,
                SpaceMap = new SpaceMap(width, height),
                BeingMap = new BeingMap(),
                ItemMap = new ItemMap(),
                RotMap = new RotMap(),
            };

            var tilledSoil = Atlas.Legend[TerrainEnum.SoilTilled];

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
                        IsTilled = (type == tilledSoil),
                    };
                    map.SpaceMap.Add(space, (x, y));
                }
            }

            foreach (var coord in data.AreaRots.Keys)
            {
                map.RotMap.Add(data.AreaRots[coord], coord);
            }

            foreach (var overlay in data.RotOverlays)
            {
                int x_off = overlay.Location.X;
                int y_off = overlay.Location.Y;
                var w = overlay.Terrain.Max(t => t.Length);
                var h = overlay.Terrain.Count;
                for (int y = 0; y < h; y++)
                {
                    var row = overlay.Terrain[y];
                    for (int x = 0; x < w; x++)
                    {
                        char symbol = (x < row.Length) ? row[x] : '.';

                        bool isD = '0' <= symbol && symbol <= '9';
                        int health = isD ? symbol - '0' : 0;
                        if (health > 0)
                            map.RotMap.Add(new AreaRot { Health = health }, (x + x_off, y + y_off));
                    }
                }
            }

            return map;
        }

        public CompoundMapData DataFromMap(CompoundMap map)
        {
            var data = new CompoundMapData
            {
                Name = map.Name,
                Width = map.Width,
                Height = map.Height,
            };

            // we don't need to put the master legend into our CMD
            //foreach (var key in Atlas.Legend.Keys)
            //{
            //    var terr = Atlas.Legend[key];
            //    var ch = ((char)terr.Cell.Glyph).ToString();
            //    data.Legend[ch] = terr.Name;
            //}

            // we want to create a list of the distinct terrains
            // used in this map
            List<string> distinctTerrainNames = new List<string>();
            foreach (var space in map.SpaceMap)
            {
                var name = space.Item.Terrain.Name;
                if (!distinctTerrainNames.Contains(name))
                {
                    distinctTerrainNames.Add(name);
                }
            }
            // ...then build the CMD legend from those
            foreach (var name in distinctTerrainNames)
            {
                var kv = Atlas.Legend.Where(t => t.Value.Name == name).Single();
                var ch = ((char)kv.Value.Cell.Glyph).ToString();
                data.Legend[ch] = name;
            }

            //data.Legend["."] = "dot dirt dere";
            //data.Legend["~"] = "tilde dirt";

            for (int y = 0; y < map.SpaceMap.Height; y++)
            {
                var sb = new StringBuilder(map.SpaceMap.Width);
                for (int x = 0; x < map.SpaceMap.Width; x++)
                {
                    var sp = map.SpaceMap.GetItem((x, y));
                    sb.Append((char)sp.Terrain.Cell.Glyph);
                }

                data.Terrain.Add(sb.ToString());
            }

            var overlay = RotOverlayDataFromMap(map);
            data.RotOverlays.Add(overlay);

            foreach (var beingST in map.BeingMap)
            {
                data.MultiBeings[beingST.Item.ID] = beingST;
            }

            foreach (var itemST in map.ItemMap)
            {
                data.MultiItems[itemST.Item.ID] = itemST;
            }

            return data;
        }

        private static RotOverlayData RotOverlayDataFromMap(CompoundMap map)
        {
            RotOverlayData overlay = new RotOverlayData();
            overlay.Name = $"{map.Name} rot";
            return overlay;
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
            var nameLI = name.ToLowerInvariant();

            if (!Atlas.Legend.ContainsKey(nameLI))
                log.Error($"Failed to find Terrain [{nameLI}].");
            return Atlas.Legend[nameLI];
        }

        #region Farm and farmhouse map YAML
        private readonly string farmMapYaml = @"---
name:  Farm
width: 46
height: 32

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

rotOverlays:
  - name: one
    location: (X=6, Y=0)
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
      - '..........111'
      - '...........111'
      - '..........11'
      - '.....1....1'
      - '...1111..11'
      - '....111111'
      - '...1123211'
      - '.111..111'
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


        internal CompoundMap DemoMap()
        {
            string demoMapYaml = @"---
name:  Demo

legend:
 '.': soil
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

            return map;
        }
    }
}
