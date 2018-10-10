using CopperBend.App.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RLNET;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CopperBend.App
{
    public enum TerrainType  //0.1
    {
        Unknown = 0,
        Dirt,
        TilledDirt,
        StoneWall,
        ClosedDoor,
        OpenDoor,
        Blight,
    }

    public class MapLoader
    {
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
                    var type = (x < row.Length)
                        ? TerrainFrom(row.Substring(x, 1))
                        : TerrainType.Unknown;

                    map.Tiles[x, y] = new Tile(x, y, type);

                    bool stoneOrClosedDoor = (
                        type == TerrainType.ClosedDoor
                     || type == TerrainType.StoneWall );

                    //TODO:  push down/unify
                    map.SetCellProperties(x, y,
                        !stoneOrClosedDoor,
                        !stoneOrClosedDoor
                    );
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
 '#': StoneWall
 '+': ClosedDoor

terrain:
 - '##################################'
 - '#................................#'
 - '#................................#'
 - '#................................#'
 - '#................................#'
 - '#................................#'
 - '#................................#'
 - '#................................#'
 - '#................................#'
 - '#................................#'
 - '#................................#'
 - '#................................#'
 - '#................................#'
 - '##################################'
";
            var map = MapFromYAML(FarmMapYaml);

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

        public TerrainType TerrainFrom(string symbol)
        {
            if (symbol == ".") return TerrainType.Dirt;
            if (symbol == "#") return TerrainType.StoneWall;
            if (symbol == "+") return TerrainType.ClosedDoor;
            return TerrainType.Unknown;
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
