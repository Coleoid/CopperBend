using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace CopperBend.App
{
    public enum TerrainType
    {
        Unknown = 0,

        Dirt = 1,
        Stone = 2,
        Blight = 9
    }

    public class MapLoader
    {
        public IkvMap LoadMap(string mapName)
        {
            if (mapName == "test:block")
                return TestBlockMap();
            else
                return MapFromYAML(mapName);
        }

        public IkvMap TestBlockMap()
        {
            var map = new KvMap(4, 4);

            map.Terrain[0, 0] = TerrainType.Stone;
            map.Terrain[0, 1] = TerrainType.Stone;
            map.Terrain[0, 2] = TerrainType.Stone;
            map.Terrain[0, 3] = TerrainType.Stone;

            map.Terrain[1, 0] = TerrainType.Stone;
            map.Terrain[1, 1] = TerrainType.Blight;
            map.Terrain[1, 2] = TerrainType.Dirt;
            map.Terrain[1, 3] = TerrainType.Stone;

            map.Terrain[2, 0] = TerrainType.Stone;
            map.Terrain[2, 1] = TerrainType.Dirt;
            map.Terrain[2, 2] = TerrainType.Dirt;
            map.Terrain[2, 3] = TerrainType.Stone;

            map.Terrain[3, 0] = TerrainType.Stone;
            map.Terrain[3, 1] = TerrainType.Stone;
            map.Terrain[3, 2] = TerrainType.Dirt;
            map.Terrain[3, 3] = TerrainType.Stone;

            return map;
        }

        public IkvMap MapFromYAML(string mapName)
        {
            return null;
        }

    }

}
