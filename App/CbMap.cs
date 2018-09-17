using RLNET;
using RogueSharp;

namespace CopperBend.App
{
    public interface IcbMap : IMap
    {
        string Name { get; set; }
        TerrainType[,] Terrain { get; set; }

        bool SetActorPosition(Actor actor, int x, int y);
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




        // Returns true when able to place the Actor on the cell or false otherwise
        public bool SetActorPosition(Actor actor, int x, int y)
        {
            // Only allow actor movement if the cell is walkable
            if (!GetCell(x, y).IsWalkable) return false;

            // The cell the actor was previously on is now walkable
            SetIsWalkable(actor.X, actor.Y, true);
            // Update the actor's position
            actor.X = x;
            actor.Y = y;
            // The new cell the actor is on is now not walkable
            SetIsWalkable(actor.X, actor.Y, false);
            // Don't forget to update the field of view if we just repositioned the player
            if (actor is Player)
            {
                UpdatePlayerFieldOfView(actor);
            }

            return true;
        }

        // This method will be called any time we move the player to update field-of-view
        public void UpdatePlayerFieldOfView(Actor player)
        {
            // Compute the field-of-view based on the player's location and awareness
            ComputeFov(player.X, player.Y, player.Awareness, true);
            // Mark all cells in field-of-view as having been explored
            foreach (Cell cell in GetAllCells())
            {
                if (IsInFov(cell.X, cell.Y))
                {
                    SetCellProperties(cell.X, cell.Y, cell.IsTransparent, cell.IsWalkable, true);
                }
            }
        }

        // Push this down to RogueSharp.Map, implement directly
        public void SetIsWalkable(int x, int y, bool isWalkable)
        {
            ICell cell = GetCell(x, y);
            SetCellProperties(cell.X, cell.Y, cell.IsTransparent, isWalkable, cell.IsExplored);
        }
    }
}
