using RogueSharp;

namespace CopperBend.App
{
    public class KvMap : Map, IkvMap
    {
        public KvMap(int xWidth, int yHeight)
            : base(xWidth, yHeight)
        {
            Terrain = new TerrainType[xWidth, yHeight];
        }
        public TerrainType [,] Terrain { get; set; }

        public TerrainType this[int x, int y]
        {
            get => Terrain[x, y];
        }
    }

}
