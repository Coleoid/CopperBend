using System.Collections.Generic;
using RLNET;
using RogueSharp;

namespace CopperBend.App
{
    public interface IcbMap : IMap
    {
        string Name { get; set; }
        TerrainType[,] Terrain { get; set; }
        List<Actor> Actors { get; set; }


        bool SetActorPosition(Actor actor, int x, int y);
        void UpdatePlayerFieldOfView(Actor player);
        void Draw(RLConsole mapConsole);
    }

    public class CbMap : Map, IcbMap
    {
        public CbMap(int xWidth, int yHeight)
            : base(xWidth, yHeight)
        {
            Terrain = new TerrainType[xWidth, yHeight];
            Actors = new List<Actor>();
        }

        public string Name { get; set; }
        public TerrainType[,] Terrain { get; set; }

        public List<Actor> Actors { get; set; }

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
                DrawCell(mapConsole, cell);
            }

            foreach (var actor in Actors)
            {
                DrawActor(mapConsole, actor);
                //...
            }
        }

        private void DrawCell(RLConsole console, ICell cell)
        {
            if (!cell.IsExplored) return;  // unknown is undrawn

            var terrain = Terrain[cell.X, cell.Y];
            var rep = TileRepresentation.OfTerrain(terrain);
            rep.IsInFOV = IsInFov(cell.X, cell.Y);

            console.Set(cell.X, cell.Y, rep.Foreground, rep.Background, rep.Symbol);
        }

        private void DrawActor(RLConsole console, Actor actor)
        {
            var cell = GetCell(actor.X, actor.Y);
            if (!cell.IsExplored) return;  // unknown is undrawn

            if (IsInFov(actor.X, actor.Y))
            {
                console.Set(actor.X, actor.Y, actor.Color, Colors.FloorBackgroundSeen, actor.Symbol, 2);
            }
            else
            {
                //0.1: Upgrade by using terrain of cell
                console.Set(actor.X, actor.Y, Colors.Floor, Colors.FloorBackground, '?', 2);
            }
        }




        // Returns true when target cell is walkable and move succeeds
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
