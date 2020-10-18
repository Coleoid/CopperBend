using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CopperBend.Creation
{
    public class MapLoader
    {
        private readonly ILog log;
        public Atlas Atlas { get; }
        [InjectProperty] private Equipper Equipper { get; set; }

        public MapLoader(ILog logger, Atlas atlas, IBeingCreator creator)
            : this(logger, atlas)
        { }

        public MapLoader(ILog logger, Atlas atlas)
        {
            log = logger;
            Atlas = atlas;
        }

        public CompoundMap MapFromYAML(string mapYaml)
        {
            CompoundMapBridge bridge = BridgeFromYAML(mapYaml);
            CompoundMap map = MapFromBridge(bridge);
            return map;
        }

        public string YAMLFromMap(ICompoundMap map)
        {
            var bridge = BridgeFromMap(map);
            var yaml = YAMLFromBridge(bridge);
            return yaml;
        }

        public IDeserializer GetDeserializer()
        {
            return new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTagMapping("!CompoundMapBridge", typeof(CompoundMapBridge))
                .WithTagMapping("!SpaceMap", typeof(SpaceMap))
                .WithTagMapping("!RotMap", typeof(RotMap))
                .WithTagMapping("!AreaRot", typeof(AreaRot))
                .WithTagMapping("!Item", typeof(Item))
                .WithTagMapping("!AttackMethod", typeof(AttackMethod))
                .WithTagMapping("!Coord_IBeing", typeof(Coord_IBeing))
                .WithTagMapping("!AttackEffect", typeof(AttackEffect))
                .WithTagMapping("!Being", typeof(Being))
                .WithTagMapping("!Trigger", typeof(Trigger))
                //.WithTypeConverter(new YTC_IBeingStrategy())
                .WithTypeConverter(new YTC_Coord())
                     .Build();
        }

        public CompoundMapBridge BridgeFromYAML(string mapYaml)
        {
            using var reader = new StringReader(mapYaml);
            var deserializer = GetDeserializer();

            var mapData = deserializer.Deserialize<CompoundMapBridge>(reader);
            return mapData;
        }

        public ISerializer GetSerializer()
        {
            return new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithEmissionPhaseObjectGraphVisitor(args =>
                    new YamlVisitor_OmitEmptyCollections(args.InnerVisitor))
                .WithEventEmitter(nextEmitter => new YamlEmitter_QuoteBuggedStrings(nextEmitter))
                .WithTagMapping("!CompoundMapBridge", typeof(CompoundMapBridge))
                .WithTagMapping("!SpaceMap", typeof(SpaceMap))
                .WithTagMapping("!RotMap", typeof(RotMap))
                .WithTagMapping("!AreaRot", typeof(AreaRot))
                .WithTagMapping("!Item", typeof(Item))
                .WithTagMapping("!AttackMethod", typeof(AttackMethod))
                .WithTagMapping("!AttackEffect", typeof(AttackEffect))
                .WithTagMapping("!Coord_IBeing", typeof(Coord_IBeing))
                .WithTagMapping("!Being", typeof(Being))
                .WithTagMapping("!Trigger", typeof(Trigger))
                //.WithTypeConverter(new YTC_IBeingStrategy())
                .WithTypeConverter(new YTC_Coord())
                .Build();
        }

        public string YAMLFromBridge(CompoundMapBridge bridge)
        {
            var serializer = GetSerializer();

            var yaml = serializer.Serialize(bridge);
            return yaml;
        }

        public CompoundMap MapFromBridge(CompoundMapBridge bridge)
        {
            var width = bridge.Terrain.Max(t => t.Length);
            var height = bridge.Terrain.Count;
            if (bridge.Width != width)
                log.Error($"Map {bridge.Name} declares width {bridge.Width} but has terrain {width} wide.");
            if (bridge.Height != height)
                log.Error($"Map {bridge.Name} declares height {bridge.Height} but has terrain {height} high.");

            var map = new CompoundMap
            {
                Name = bridge.Name,
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
                string row = bridge.Terrain[y];
                for (int x = 0; x < width; x++)
                {
                    var symbol = (x < row.Length)
                        ? row.Substring(x, 1)
                        : "???";
                    var name = bridge.Legend[symbol];

                    var type = TerrainFrom(name);
                    Space space = new Space
                    {
                        Terrain = type,
                        IsTilled = (type == tilledSoil),
                    };
                    map.SpaceMap.Add(space, (x, y));
                }
            }

            foreach (var coord in bridge.AreaRots.Keys)
            {
                map.RotMap.Add(bridge.AreaRots[coord], coord);
            }

            foreach (var pair in bridge.AreaRots)
            {
                map.RotMap.Add(pair.Value, pair.Key);
            }

            map.Triggers.AddRange(bridge.Triggers);

            return map;
        }

        public CompoundMapBridge BridgeFromMap(ICompoundMap map)
        {
            var bridge = new CompoundMapBridge
            {
                Name = map.Name,
                Width = map.Width,
                Height = map.Height,
            };

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
            // ...then build the bridge legend from those
            foreach (var name in distinctTerrainNames)
            {
                var kv = Atlas.Legend.Where(t => t.Value.Name == name).Single();
                var ch = ((char)kv.Value.Cell.Glyph).ToString();
                bridge.Legend[ch] = name;
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

                bridge.Terrain.Add(sb.ToString());
            }

            foreach (var beingST in map.BeingMap)
            {
                bridge.MultiBeings[beingST.Item.ID] = new Coord_IBeing { Coord = beingST.Position, Being = beingST.Item };
            }

            foreach (var itemST in map.ItemMap)
            {
                bridge.MultiItems[itemST.Item.ID] = new Coord_IItem { Coord = itemST.Position, Item = itemST.Item };
            }

            foreach (var rotST in map.RotMap)
            {
                bridge.AreaRots.Add(rotST.Position, rotST.Item);
            }

            bridge.Triggers.AddRange(map.Triggers);

            return bridge;
        }

        //private static List<RotOverlayData> RotOverlayDataFromMap(ICompoundMap map)
        //{
        //    var overlays = new List<RotOverlayData>();
        //    if (!map.RotMap.Any()) return overlays;

        //    var minX = int.MaxValue;
        //    var minY = int.MaxValue;
        //    var maxX = int.MinValue;
        //    var maxY = int.MinValue;

        //    foreach (var ar in map.RotMap)
        //    {
        //        // Bounding coords
        //        minX = Math.Min(ar.Position.X, minX);
        //        minY = Math.Min(ar.Position.Y, minY);
        //        maxX = Math.Max(ar.Position.X, maxX);
        //        maxY = Math.Max(ar.Position.Y, maxY);

        //        // List of health values
        //    }
        //    var overlay = new RotOverlayData();
        //    overlay.Name = $"{map.Name} rot";
        //    overlay.Location = (minX, minY);

        //    var tr = new StringBuilder((maxX - minX) * 2);
        //    var hl = new List<int>();
        //    for (var y = minY; y < maxY; y++)
        //    {
        //        tr.Clear();
        //        for (var x = minX; x < maxX; x++)
        //        {
        //            var ar = map.RotMap.GetItem((x, y));
        //            string repr = ReprOf(ar);
        //            tr.Append(repr);
        //        }
        //    }

        //    string ReprOf(IAreaRot ar)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    var ints = new List<int>();
        //    //0.1 Only returns empty overlay
        //    //case: No current AreaRot on map
        //    //case: too many health levels for easy char-per representation

        //    //GUH!  overlay.Terrain

        //    return overlays;
        //}


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

                var rots = new List<string> {
                    "..1221",
                    "...1211",
                    "....121",
                    ".1112211",
                    "11122221111...11",
                    ".1111232221111111",
                    ".11113332221111",
                    "111111333221111",
                    "11111333221111",
                    ".1111333111.11",
                    ".1133221111.11",
                    ".1123232111",
                    ".112111331111",
                    ".112111122111",
                    ".1111 111111",
                    "..1........1",
                    "..11......11",
                    "..11......1",
                    "..........111",
                    "...........111",
                    "..........11",
                    ".....1....1",
                    "...1111..11",
                    "....111111",
                    "...1123211",
                    ".111..111",
                    ".......1",
                };
                AddRotsFromTextList(farmMap.RotMap, (6, 0), rots);

                //  Obscure point on the edge to test map transitions
                //_farmMap.AddEventAtLocation(new Point(41, 1), new CommandEntry(GameCommand.GoToFarmhouse, null));

                //  Barrier in front of the gate out
                var noLeave = new Trigger
                {
                    Name = "Not Ready to Leave",
                    Categories = TriggerCategories.PlayerLocation,
                    Condition = "(5, 16) to (5, 18)",
                    Script = new List<string>
                    {
                        "message",
                        "Something needs doing here, first.",
                        "end message",
                        "move player: (6, 17)",
                    },
                };
                farmMap.AddTrigger(noLeave);
            }

            log.Debug("Loaded the farmyard map");
            return farmMap;
        }

        public void AddRotsFromTextList(IRotMap map, Coord offset, List<string> text)
        {
            if (!text.Any())
                throw new Exception($"Nothing to add to RotMap");

            var w = text.Max(t => t.Length);
            var h = text.Count;
            for (int y = 0; y < h; y++)
            {
                var row = text[y];
                for (int x = 0; x < w; x++)
                {
                    char symbol = (x < row.Length) ? row[x] : '.';

                    bool isD = '0' <= symbol && symbol <= '9';
                    int health = isD ? symbol - '0' : 0;
                    if (health > 0)
                        map.Add(new AreaRot { Health = health }, (x + offset.X, y + offset.Y));
                }
            }
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
name:  Tacker Farm
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

triggers:
- !Trigger
  name: Wake up
  condition: (0, 0) to (99, 99)
  script:
  - move player (22, 17)
  - message
  - I wake up.  Cold--frost on the ground, except where I was lying.
  - Everything hurts when I stand up.
  - The sky... says it's morning.  A small farmhouse to the east.
  - Something real wrong with the ground to the west, and the northwest.
  - end message
  - remove trigger
  categories: MapChanged
- !Trigger
  name: Walk into farmhouse
  condition: (33, 13) to (34, 15)
  script:
  - message
  - Nobody's been here for a while.  Dust... closed-off smell...
  - Looking right, light's leaking through a corner of the roof.
  - Lost a couple shingles, I guess.
  - end message
  - remove trigger
  categories: PlayerLocation

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
