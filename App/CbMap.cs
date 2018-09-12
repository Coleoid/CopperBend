using RLNET;
using RogueSharp;

namespace CopperBend.App
{
    public interface IcbMap : IMap
    {
        string Name { get; set; }
        TerrainType[,] Terrain { get; set; }
    }

    public class CbMap : Map, IcbMap
    {
        public CbMap(int xWidth, int yHeight)
            : base(xWidth, yHeight)
        {
            Terrain = new TerrainType[xWidth, yHeight];
        }

        public string Name { get; set; }
        public TerrainType[,] Terrain { get; set; }

        public TerrainType this[int x, int y]
        {
            get => Terrain[x, y];
        }

        public bool IsTillable(int x, int y)
        {
            return Terrain[x, y] == TerrainType.Dirt;
        }
        public bool IsTillable(Cell cell) => IsTillable(cell.X, cell.Y);

        public void Draw(RLConsole mapConsole)
        {
            mapConsole.Clear();
            foreach (ICell cell in GetAllCells())
            {
                SetConsoleSymbolForCell(mapConsole, cell);
            }
        }

        private void SetConsoleSymbolForCell(RLConsole console, ICell cell)
        {
            if (!cell.IsExplored) return;  // unknown is undrawn

            var terrain = Terrain[cell.X, cell.Y];
            var rep = TileRepresentation.OfTerrain(terrain);
            rep.IsInFOV = IsInFov(cell.X, cell.Y);

            console.Set(cell.X, cell.Y, rep.Foreground, rep.Background, rep.Symbol);
        }
    }
}
