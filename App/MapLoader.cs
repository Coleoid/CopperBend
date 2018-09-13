using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CopperBend.App
{
    public enum TerrainType
    {
        Unknown = 0,
        Dirt,
        StoneWall,
        Door,
        Blight,
    }

    public class MapLoader
    {
        public IcbMap LoadMap(string mapName)
        {
            return MapFromFile(mapName);
        }

        public IcbMap MapFromFile(string mapName)
        {
            return null;
        }

        public IcbMap MapFromYAML(string mapYaml)
        {
            var dto = DTOFromYAML(mapYaml);
            var width = dto.Terrain.Max(t => t.Length);
            var height = dto.Terrain.Count();
            var map = new CbMap(width, height);

            map.Name = dto.Name;

            for (int y = 0; y < height; y++)
            {
                string row = dto.Terrain[y];
                for (int x = 0; x < width; x++)
                {
                    map.Terrain[x, y] = (x < row.Length)
                        ? TerrainFrom(row.Substring(x, 1))
                        : TerrainType.Unknown;
                    //map.SetCellProperties();
                }
            }

            return map;
        }

        public TerrainType TerrainFrom(string symbol)
        {
            //0.1
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
