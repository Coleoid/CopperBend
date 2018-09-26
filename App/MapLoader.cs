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
        Door,
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
            var dto = DTOFromYAML(mapYaml);
            var width = dto.Terrain.Max(t => t.Length);
            var height = dto.Terrain.Count();
            var map = new AreaMap(width, height);

            map.Name = dto.Name;

            for (int y = 0; y < height; y++)
            {
                string row = dto.Terrain[y];
                for (int x = 0; x < width; x++)
                {
                    var type = (x < row.Length)
                        ? TerrainFrom(row.Substring(x, 1))
                        : TerrainType.Unknown;

                    map.Tiles[x, y] = new Tile(x, y, type);

                    //TODO:  push down/unify
                    map.SetCellProperties(x,y,
                        type != TerrainType.StoneWall
                          && type != TerrainType.Door,
                        type != TerrainType.StoneWall
                        );
                }
            }

            return map;
        }

        internal IAreaMap DemoMap()
        {
            string DemoMapYaml = @"---
name:  Demo

legend:
 '.': Dirt
 '#': StoneWall
 '+': Door

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

            return map;
        }

        public TerrainType TerrainFrom(string symbol)
        {
            if (symbol == ".") return TerrainType.Dirt;
            if (symbol == "#") return TerrainType.StoneWall;
            if (symbol == "+") return TerrainType.Door;
            return TerrainType.Unknown;
        }

        public MapDTO DTOFromYAML(string mapYaml)
        {
            var reader = new StringReader(mapYaml);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            return deserializer.Deserialize<MapDTO>(reader);
        }
    }

    public class MapDTO
    {
        public string Name { get; set; }
        public Dictionary<string, string> Legend { get; set; }
        public List<string> Terrain { get; set; }
    }
}
