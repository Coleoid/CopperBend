using System.Collections.Generic;
using System.Linq;
using RLNET;
using CopperBend.MapUtil;
using System;

namespace CopperBend.App
{
    public struct CommandEntry
    {
        public CommandEntry(GameCommand command, Action<IControlPanel> doAction)
        {
            Command = command;
            DoAction = doAction;
        }

        public readonly GameCommand Command;
        public readonly Action<IControlPanel> DoAction;
    }

    public class AreaMap : Map, IAreaMap
    {
        public AreaMap(int xWidth, int yHeight)
            : base(xWidth, yHeight)
        {
            Tiles = new ITile[xWidth, yHeight];
            Actors = new List<IActor>();
            Items = new List<IItem>();
            FirstSightMessages = new List<string>();
            LocationMessages = new Dictionary<Point, List<string>>();
            LocationEventEntries = new Dictionary<Point, List<CommandEntry>>();
            DisplayDirty = true;
        }

        public List<string> FirstSightMessages { get; set; }
        public Dictionary<Point, List<string>> LocationMessages { get; private set; }
        public Dictionary<Point, List<CommandEntry>> LocationEventEntries { get; private set; }

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

        public bool DisplayDirty { get; set; }
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
            if (!IsExplored(point)) return;  // unknown is unshown
            var tile = this[point];

            tile.IsInFOV = IsInFov(point);
            var bgColor = tile.ColorBackground;
            var fgColor = tile.ColorForeground;
            var symbol = tile.Symbol;

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


        // Returns true when target cell is walkable and move succeeds
        public bool MoveActor(IActor actor, Point targetPoint)
        {
            // Only allow actor movement if the cell is walkable
            if (!GetCell(targetPoint).IsWalkable) return false;

            var startPoint = actor.Point;
            SetIsWalkable(startPoint, true);
            actor.MoveTo(targetPoint);
            SetIsWalkable(targetPoint, false);
            DisplayDirty = true;

            return true;
        }

        //  Player field of view changes whenever player moves
        //FUTURE: more cases (shifting terrain, smoke cloud, et c.)
        public void UpdatePlayerFieldOfView(IActor actor)
        {
            var fovCells = ComputeFov(actor.Point, actor.Awareness, true);

            foreach (Cell cell in fovCells)
            {
                SetIsExplored(cell.Point, true);
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
            SetIsTransparent(tile.Point, true);
            SetIsWalkable(tile.Point, true);
            DisplayDirty = true;
            UpdatePlayerFieldOfView(ViewpointActor);
        }

        public bool HasEventAtPoint(Point point)
        {
            return LocationMessages.ContainsKey(point)
                || LocationEventEntries.ContainsKey(point);
        }

        public void RunEvent(IActor player, ITile tile, IControlPanel controls)
        {
            if (LocationMessages.ContainsKey(tile.Point))
            {
                var message = LocationMessages[tile.Point];
                foreach (var line in message)
                    controls.WriteLine(line);

                LocationMessages.Remove(tile.Point);
            }

            //0.2
            if (LocationEventEntries.ContainsKey(tile.Point))
            {
                var entries = LocationEventEntries[tile.Point];
                foreach (var entry in entries)
                {
                    controls.QueueCommand(entry.Command);
                }
            }

            //0.3 may unify those collections and loops, may restructure flow
        }

        public void AddEventAtLocation(Point point, CommandEntry entry)
        {
            if (!LocationEventEntries.ContainsKey(point))
            {
                LocationEventEntries[point] = new List<CommandEntry>();
            }

            var list = LocationEventEntries[point];
            list.Add(entry);

        }

         public Point PlayerStartsAt { get; set; }
    }
}
