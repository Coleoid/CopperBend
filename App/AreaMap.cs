using System.Collections.Generic;
using System.Linq;
using RLNET;
using CopperBend.MapUtil;

namespace CopperBend.App
{
    public class AreaMap : Map, IAreaMap
    {
        public AreaMap(int xWidth, int yHeight)
            : base(xWidth, yHeight)
        {
            Tiles = new ITile[xWidth, yHeight];
            TileTypes = new Dictionary<string, TileType>();
            Actors = new List<IActor>();
            Items = new List<IItem>();
            FirstSightMessage = new List<string>();
            LocationMessages = new Dictionary<Point, List<string>>();
            //LocationEventEntries = new Dictionary<Point, List<CommandEntry>>();
            IsDisplayDirty = true;
        }

        public List<string> FirstSightMessage { get; set; }
        public Dictionary<Point, List<string>> LocationMessages { get; private set; }
        //public Dictionary<Point, List<CommandEntry>> LocationEventEntries { get; private set; }

        public string Name { get; set; }
        public ITile[,] Tiles { get; set; }

        public List<IItem> Items { get; set; }
        public List<IActor> Actors { get; set; }

        public IActor ViewpointActor { get; set; }
        public Dictionary<string, TileType> TileTypes { get; set; }

        public ITile this[Point point] => Tiles[point.X, point.Y];

        public bool IsTillable(Point point) => this[point].IsTillable;
        public bool IsTillable(int x, int y) => Tiles[x, y].IsTillable;
        public bool IsTillable(Cell cell) => this[cell.Point].IsTillable;

        public bool IsDisplayDirty { get; set; }
        public void DrawMap(RLConsole mapConsole)
        {
            mapConsole.Clear();
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    DrawLocation(mapConsole, new Point(x, y));
                }
            }
        }

        private void DrawLocation(RLConsole console, Point point)
        {
            if (!IsExplored(point)) return;  // unknown is undrawn
            var tile = this[point];

            tile.IsInFOV = IsInFov(point);
            var bgColor = tile.ColorBackground;
            var fgColor = tile.ColorForeground;
            var symbol = tile.Symbol;

            //FUTURE:  Also level of illumination on tile
            //...which is where we need to go for remote sensing via allied plant
            if (tile.IsInFOV)  //  If we can see this tile, we can see actors or items on it
            {
                var actor = Actors.Where(a => a.Point.Equals(point)).SingleOrDefault();
                if (actor != null)
                {
                    fgColor = actor.ColorForeground;
                    symbol = actor.Symbol;
                }
                else
                {
                    var topItem = Items.Where(i => i.Point.Equals(point)).FirstOrDefault();
                    if (topItem != null)
                    {
                        fgColor = topItem.ColorForeground;
                        symbol = topItem.Symbol;
                    }
                }
            }

            RelativeDraw(console, point, fgColor, bgColor, symbol);
        }

        private void RelativeDraw(RLConsole console, Point absolutePoint, RLColor fgColor, RLColor bgColor, char symbol)
        {
            var aX = absolutePoint.X - ViewpointActor.Point.X + console.Width / 2;
            var aY = absolutePoint.Y - ViewpointActor.Point.Y + console.Height / 2;
            if (aX >= 0 && aX < console.Width && aY >= 0 && aY < console.Height)
            {
                console.Set(aX, aY, fgColor, bgColor, symbol);
            }
        }


        public void MoveActor(IActor actor, Point targetPoint)
        {
            Guard.Against(!GetCell(targetPoint).IsWalkable, 
                "Must only move onto walkable locations");

            var startPoint = actor.Point;
            SetWalkable(startPoint, true);
            actor.MoveTo(targetPoint);
            SetWalkable(targetPoint, false);
            IsDisplayDirty = true;
        }

        //  Player field of view changes whenever player moves
        //FUTURE: more trigger cases (shifting terrain, smoke cloud, et c.)
        public void UpdatePlayerFieldOfView(IActor actor)
        {
            var fovCells = ComputeFov(actor.Point, actor.Awareness, true);

            foreach (Cell cell in fovCells)
            {
                SetExplored(cell.Point, true);
            }
        }

        public IActor GetActorAtPoint(Point point)
        {
            return Actors
                .Where(a => a.Point.Equals(point))
                .FirstOrDefault();
        }

        public void OpenDoor(ITile tile)
        {
            Guard.Against(tile.TileType.Name != "closed door");
            tile.SetTileType(TileTypes["open door"]);
            SetTransparent(tile.Point, true);
            SetWalkable(tile.Point, true);
            IsDisplayDirty = true;
            UpdatePlayerFieldOfView(ViewpointActor);
        }

        public bool HasEventAtPoint(Point point)
        {
            return LocationMessages.ContainsKey(point);
        }

         public Point PlayerStartsAt { get; set; }

        public void SetTile(ITile tile)
        {
            Tiles[tile.Point.X, tile.Point.Y] = tile;
            SetTransparent(tile.Point, tile.TileType.IsTransparent);
            SetWalkable(tile.Point, tile.TileType.IsWalkable);

            if (!TileTypes.ContainsKey(tile.TileType.Name))
            {
                TileTypes[tile.TileType.Name] = tile.TileType;
            }
        }
    }
}
