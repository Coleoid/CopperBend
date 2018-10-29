using System.Collections.Generic;
using System.Linq;
using RLNET;
using RogueSharp;

namespace CopperBend.App
{
    public class AreaMap : Map, IAreaMap
    {
        public AreaMap(int xWidth, int yHeight)
            : base(xWidth, yHeight)
        {
            Tiles = new ITile[xWidth, yHeight];
            Actors = new List<IActor>();
            Items = new List<IItem>();
            FirstSightMessages = new List<string>();
            LocationMessages = new Dictionary<Coord, List<string>>();
            DisplayDirty = true;
        }

        public string Name { get; set; }
        public ITile[,] Tiles { get; set; }

        public List<IItem> Items { get; set; }
        public List<IActor> Actors { get; set; }

        public IActor ViewpointActor { get; set; }
        public Dictionary<string, TileType> TileTypes { get; set; }

        public ITile this[Coord coord] => Tiles[coord.X, coord.Y];

        public bool IsTillable(Coord coord) => this[coord].IsTillable;
        public bool IsTillable(int x, int y) => Tiles[x, y].IsTillable;
        public bool IsTillable(Cell cell) => this[cell.Coord].IsTillable;

        public bool DisplayDirty { get; set; }
        public void DrawMap(RLConsole mapConsole)
        {
            mapConsole.Clear();
            foreach (Cell cell in GetAllCells())
            {
                DrawCoord(mapConsole, cell.Coord);
            }
        }

        private void DrawCoord(RLConsole console, Coord coord)
        {
            if (!IsExplored(coord)) return;  // unknown is unshown
            var tile = this[coord];

            tile.IsInFOV = IsInFov(coord);
            var bgColor = tile.ColorBackground;
            var fgColor = tile.ColorForeground;
            var symbol = tile.Symbol;

            if (tile.IsInFOV)  //  If we can see this tile, we can see actors or items on it
            {
                var actor = Actors.Where(a => a.Coord.Equals(coord)).SingleOrDefault();
                if (actor != null)
                {
                    fgColor = actor.ColorForeground;
                    symbol = actor.Symbol;
                }
                else
                {
                    var topItem = Items.Where(i => i.Coord.Equals(coord)).FirstOrDefault();
                    if (topItem != null)
                    {
                        fgColor = topItem.ColorForeground;
                        symbol = topItem.Symbol;
                    }
                }
            }

            RelativeDraw(console, coord, fgColor, bgColor, symbol);
        }

        private void RelativeDraw(RLConsole console, Coord absoluteCoord, RLColor fgColor, RLColor bgColor, char symbol)
        {
            var aX = absoluteCoord.X - ViewpointActor.Coord.X + console.Width / 2;
            var aY = absoluteCoord.Y - ViewpointActor.Coord.Y + console.Height / 2;
            if (aX >= 0 && aX < console.Width && aY >= 0 && aY < console.Height)
            {
                console.Set(aX, aY, fgColor, bgColor, symbol);
            }
        }


        // Returns true when target cell is walkable and move succeeds
        public bool MoveActor(IActor actor, Coord targetCoord)
        {
            // Only allow actor movement if the cell is walkable
            if (!GetCell(targetCoord).IsWalkable) return false;

            var startCoord = actor.Coord;
            SetIsWalkable(startCoord, true);
            actor.MoveTo(targetCoord);
            SetIsWalkable(targetCoord, false);
            DisplayDirty = true;

            return true;
        }

        //  Player field of view changes whenever player moves
        //FUTURE: more cases (shifting terrain, smoke cloud, et c.)
        public void UpdatePlayerFieldOfView(IActor actor)
        {
            var fovCells = ComputeFov(actor.Coord.X, actor.Coord.Y, actor.Awareness, true);

            foreach (Cell cell in fovCells)
            {
                SetIsExplored(cell.Coord, true);
            }
        }

        public IActor GetActorAtCoord(Coord coord)
        {
            return Actors
                .Where(a => a.Coord.Equals(coord))
                .FirstOrDefault();
        }

        public void OpenDoor(ITile tile)
        {
            Guard.Against(tile.TileType.Name != "ClosedDoor");
            tile.SetTileType(TileTypes["OpenDoor"]);
            SetIsTransparent(tile.Coord, true);
            SetIsWalkable(tile.Coord, true);
            DisplayDirty = true;
            UpdatePlayerFieldOfView(ViewpointActor);
        }

        public bool HasEventAtCoords(Coord coord)
        {
            var farmhouseDoor = new Coord(27, 13);
            if (coord.Equals(farmhouseDoor))
                return true;

            return LocationMessages.ContainsKey(coord);
        }

        public void RunEvent(IActor player, ITile tile, IControlPanel controls)
        {
            var message = LocationMessages[tile.Coord];
            foreach (var line in message)
                controls.WriteLine(line);
        }

        public List<string> FirstSightMessages { get; set; }
        public Dictionary<Coord, List<string>> LocationMessages { get; private set; }
    }
}
