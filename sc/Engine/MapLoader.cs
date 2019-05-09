﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CopperBend.Contract;
using GoRogue;
using Microsoft.Xna.Framework;
using SadConsole;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CopperBend.Engine
{
    public class MapLoader
    {
        Dictionary<string, TerrainType> TerrainTypes;

        public MapLoader()
        {
            InitTerrain();
        }

        private void InitTerrain()
        {
            TerrainTypes = new Dictionary<string, TerrainType>();

            var type = new TerrainType
            {
                Name = "Unknown",
                CanWalkThrough = true,
                CanSeeThrough = true,
                Looks = new Cell(Color.DarkRed, Color.DarkRed, '?'),
            };
            StoreTerrainType(type);

            type = new TerrainType
            {
                Name = "dirt",
                CanWalkThrough = true,
                CanSeeThrough = true,
                CanPlant = true,
                Looks = new Cell(Color.DarkGray, Color.Gray, '.'),
            };
            StoreTerrainType(type);

            type = new TerrainType
            {
                Name = "tilled dirt",
                CanWalkThrough = true,
                CanSeeThrough = true,
                CanPlant = true,
                Looks = new Cell(Color.Brown, Color.SandyBrown, '~'),

            };
            StoreTerrainType(type);


            type = new TerrainType
            {
                Name = "stone wall",
                CanWalkThrough = false,
                CanSeeThrough = false,
                Looks = new Cell(Color.DarkGray, Color.SlateGray, '#'),
            };
            StoreTerrainType(type);

            type = new TerrainType
            {
                Name = "closed door",
                CanWalkThrough = false,
                CanSeeThrough = false,
                Looks = new Cell(Color.DarkGray, Color.SlateGray, '+'),
            };
            StoreTerrainType(type);

            type = new TerrainType
            {
                Name = "open door",
                CanWalkThrough = true,
                CanSeeThrough = true,
                Looks = new Cell(Color.DarkGray, Color.SlateGray, '-'),
            };
            StoreTerrainType(type);

            type = new TerrainType
            {
                Name = "wooden fence",
                CanWalkThrough = false,
                CanSeeThrough = false,
                Looks = new Cell(Color.Brown, Color.SandyBrown, 'X'),
            };
            StoreTerrainType(type);

            type = new TerrainType
            {
                Name = "wall",
                CanWalkThrough = false,
                CanSeeThrough = false,
                Looks = new Cell(Color.DarkGray, Color.SlateGray, '='),
            };
            StoreTerrainType(type);

            type = new TerrainType
            {
                Name = "gate",
                CanWalkThrough = false,
                CanSeeThrough = true,
                Looks = new Cell(Color.DarkGray, Color.SlateGray, '%'),
            };
            StoreTerrainType(type);

            type = new TerrainType
            {
                Name = "grass",
                CanWalkThrough = true,
                CanSeeThrough = true,
                CanPlant = true,
                Looks = new Cell(Color.LightGreen, Color.LawnGreen, ','),
            };
            StoreTerrainType(type);

            type = new TerrainType
            {
                Name = "tall weeds",
                CanWalkThrough = true,
                CanSeeThrough = true,
                Looks = new Cell(Color.LightGreen, Color.LawnGreen, 'w'),
            };
            StoreTerrainType(type);

            type = new TerrainType
            {
                Name = "table",
                CanWalkThrough = false,
                CanSeeThrough = true,
                Looks = new Cell(Color.BurlyWood, Color.Wheat, 'T'),
            };
            StoreTerrainType(type);

            type = new TerrainType()
            {
                Name = "stairs",
                CanWalkThrough = true,
                CanSeeThrough = true,
                Looks = new Cell(Color.AliceBlue, Color.DarkGoldenrod, '>'),
            };
            StoreTerrainType(type);
        }

        public void StoreTerrainType(TerrainType type)
        {
            if (TerrainTypes.ContainsKey(type.Name))
                throw new Exception($"Already have type {type.Name} stored.");

            TerrainTypes[type.Name] = type;
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

            //Attach(map, state);

            return map;
        }

        //public void Attach(IAreaMap map, GameState state)
        //{
        //    map.ViewpointActor = state.Player;
        //    map.Actors.Add(state.Player);
        //    state.Player.MoveTo(map.PlayerStartsAt);
        //    map.UpdatePlayerFieldOfView(state.Player);
        //}

        public IAreaMap LoadMap(string mapName)
        {
            return MapFromFile(mapName);
        }

        public IAreaMap MapFromFile(string mapName)
        {
            return null;
        }

        public CompoundMap MapFromYAML(string mapYaml)
        {
            var data = DataFromYAML(mapYaml);
            var width = data.Terrain.Max(t => t.Length);
            var height = data.Terrain.Count();

            var map = new CompoundMap();
            var spaceMap = new SpaceMap(width, height);

            var tilledType = TerrainTypes["tilled dirt"];  //0.1

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
                    spaceMap.Add(space, x, y);

                    if (type == tilledType) space.Till();
                }
            }

            spaceMap.PlayerStartPoint = (23, 21);
            map.SpaceMap = spaceMap;

            var blightMap = new SpatialMap<AreaBlight>();
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
                        int extent = isD ? int.Parse(symbol) : 0;


                        blightMap.Add(new AreaBlight {Extent = extent}, x + x_off, y + y_off);
                    }
                }
            }

            map.BlightMap = blightMap;
            map.BeingMap = new MultiSpatialMap<IBeing>();
            map.ItemMap = new MultiSpatialMap<IItem>();
            map.LocatedTriggers = new List<LocatedTrigger>();

            return map;
        }

        private CompoundMap _farmMap = null;
        internal CompoundMap FarmMap()
        {
            if (_farmMap == null)
            {
                _farmMap = MapFromYAML(FarmMapYaml);
                //_farmMap.PlayerStartsAt = new Point(23, 21);

                //  Obscure point on the edge to test map transitions
                //_farmMap.AddEventAtLocation(new Point(41, 1), new CommandEntry(GameCommand.GoToFarmhouse, null));

                ////  Barrier in front of the gate out
                //_farmMap.AddEventAtLocation(new Point(5, 16), new CommandEntry(GameCommand.NotReadyToLeave, null));
                //_farmMap.AddEventAtLocation(new Point(5, 17), new CommandEntry(GameCommand.NotReadyToLeave, null));
                //_farmMap.AddEventAtLocation(new Point(5, 18), new CommandEntry(GameCommand.NotReadyToLeave, null));
            }

            return _farmMap;
        }

        private CompoundMap _farmhouseMap = null;
        internal CompoundMap FarmhouseMap()
        {
            if (_farmhouseMap == null)
            {
                _farmhouseMap = MapFromYAML(RootCellarYaml);
                //_farmhouseMap.PlayerStartsAt = new Point(2, 7);
            }

            return _farmhouseMap;
        }

        public TerrainType TerrainFrom(string name)
        {
            name = name.ToLower();
            var foundType = TerrainTypes.ContainsKey(name) ? name : "Unknown";
            return TerrainTypes[foundType];
        }

        public MapData DataFromYAML(string mapYaml)
        {
            var reader = new StringReader(mapYaml);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            return deserializer.Deserialize<MapData>(reader);
        }

        internal CompoundMap DemoMap()
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