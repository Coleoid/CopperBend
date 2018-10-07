using System.Collections.Generic;
using System.Linq;
using CopperBend.App.Model;
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
            DisplayDirty = true;
        }

        public string Name { get; set; }
        public ITile[,] Tiles { get; set; }

        public List<IItem> Items { get; set; }

        public List<IActor> Actors { get; set; }
        public IActor ViewpointActor { get; set; }

        public bool DisplayDirty { get; set; }


        public ITile this[ICoord coord]
        {
            get => Tiles[coord.X, coord.Y];
        }

        public bool IsTillable(int x, int y)
        {
            return Tiles[x, y].IsTillable;
        }
        public bool IsTillable(Cell cell) => IsTillable(cell.X, cell.Y);

        public void DrawMap(RLConsole mapConsole)
        {
            mapConsole.Clear();
            foreach (ICell cell in GetAllCells())
            {
                DrawCell(mapConsole, cell);
            }

            foreach (var item in Items)
            {
                Draw(mapConsole, item);
            }

            foreach (var actor in Actors)
            {
                Draw(mapConsole, actor);
            }

            //FUTURE: Instead, DrawCell draws actor, else the top item, else
            // the empty tile representation
        }

        private void DrawCell(RLConsole console, ICoord coord)
        {
            if (!IsExplored(coord)) return;  // unknown is undrawn
            var isInFOV = IsInFov(coord);

            var tile = Tiles[coord.X, coord.Y];
            var rep = ((Tile) tile).repr;  //TODO: FIXME: HACK: bleh.
            var fg = rep.Foreground(isInFOV);
            var bg = rep.Background(isInFOV);

            RelativeDraw(console, coord, fg, bg, rep.Symbol);
        }

        private void Draw(RLConsole console, IDrawable thing)
        {
            if (!IsExplored(thing)) return;  // unknown is undrawn

            IDrawable show = IsInFov(thing)
                ? thing 
                : (IDrawable)Tiles[thing.X, thing.Y];

            //TODO: for background, get tile.bg.inFOV
            RelativeDraw(console, show, show.Color, Colors.FloorBackgroundSeen, show.Symbol);
        }

        private void RelativeDraw(RLConsole console, ICoord absoluteCoord, RLColor fgColor, RLColor bgColor, char symbol)
        {
            var aX = absoluteCoord.X - ViewpointActor.X + console.Width / 2;
            var aY = absoluteCoord.Y - ViewpointActor.Y + console.Height / 2;
            if (aX >= 0 && aX < console.Width && aY >= 0 && aY < console.Height)
            {
                console.Set(aX, aY, fgColor, bgColor, symbol);
            }
        }


        // Returns true when target cell is walkable and move succeeds
        public bool SetActorPosition(IActor actor, int x, int y)
        {
            // Only allow actor movement if the cell is walkable
            if (!GetCell(x, y).IsWalkable) return false;

            // Update the actor's position
            SetIsWalkable(actor, true);
            actor.MoveTo(x, y);
            SetIsWalkable(actor, false);
            DisplayDirty = true;

            return true;
        }

        public bool SetActorCoord(IActor actor, ICoord coord)
            => SetActorPosition(actor, coord.X, coord.Y);

        //  Player field of view changes whenever player moves
        //FUTURE: more cases (shifting terrain, smoke cloud, et c.)
        public void UpdatePlayerFieldOfView(IActor player)
        {
            var fovCells = ComputeFov(player.X, player.Y, player.Awareness, true);

            foreach (Cell cell in fovCells)
            {
                SetIsExplored(cell, true);
            }
        }

        public IActor GetActorAtPosition(int x, int y)
        {
            return Actors
                .Where(a => a.X == x && a.Y == y)
                .FirstOrDefault();
        }
        public IActor GetActorAtCoord(ICoord coord)
            => GetActorAtPosition(coord.X, coord.Y);
    }
}
