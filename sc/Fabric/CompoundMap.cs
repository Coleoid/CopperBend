using System;
using System.Collections.Generic;
using Color = Microsoft.Xna.Framework.Color;
using GoRogue;
using GoRogue.MapViews;
using CopperBend.Contract;
using System.Linq;
using SadConsole;

namespace CopperBend.Fabric
{
    public class CompoundMap : ICompoundMap
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public SpaceMap SpaceMap { get; set; }
        public MultiSpatialMap<IBeing> BeingMap { get; set; }
        public MultiSpatialMap<IItem> ItemMap { get; set; }
        public SpatialMap<AreaBlight> BlightMap { get; set; }
        public List<LocatedTrigger> LocatedTriggers { get; set; }

        public FOV FOV { get; set; }
        public bool VisibilityChanged { get; set; }
        public List<Coord> CoordsWithChanges { get; } = new List<Coord>();

        public bool CanSeeThrough(Coord location) => SpaceMap.CanSeeThrough(location);
        public bool CanWalkThrough(Coord location) => SpaceMap.CanWalkThrough(location);
        public bool CanPlant(Coord location) => SpaceMap.CanPlant(location);

        public IMapView<int> GetView_BlightStrength()
        {
            throw new NotImplementedException();
        }

        public IMapView<bool> GetView_CanSeeThrough()
        {
            return new LambdaMapView<bool>(
                () => Width, () => Height,
                (coord) => CanSeeThrough(coord)
            );
        }

        public IMapView<bool> GetView_CanWalkThrough()
        {
            throw new NotImplementedException();
        }

        public bool IsWithinMap(Coord location)
        {
            throw new NotImplementedException();
        }

        /// <summary> Set all cells to blank if unknown, or 'unseen' color of terrain if known </summary>
        public void SetInitialConsoleCells(ScrollingConsole console, SpaceMap spaceMap)
        {
            var unknownCell = new Cell(Color.Black, Color.Black, ' ');
            var knownCell = new Cell(Color.DarkGray, Color.Black, 'i');

            for (int y = 0; y < spaceMap.Height; y++)
            {
                for (int x = 0; x < spaceMap.Width; x++)
                {
                    var space = spaceMap.GetItem(x, y);
                    if (space.IsKnown)
                    {
                        knownCell.Glyph = space.Terrain.Looks.Glyph;
                        console.SetCellAppearance(x, y, knownCell);
                    }
                    else
                    {
                        console.SetCellAppearance(x, y, unknownCell);
                    }
                }
            }
        }

        public void UpdateFOV(ScrollingConsole console, Coord position)
        {
            FOV.Calculate(position);

            SpaceMap.SeeCoords(FOV.NewlySeen);

            //  Cells outside of FOV are gray on black,
            // with only the glyph of the terrain showing.
            var unseenCell = new Cell(Color.DarkGray, Color.Black, ' ');
            foreach (var location in FOV.NewlyUnseen)
            {
                unseenCell.Glyph = SpaceMap.GetItem(location).Terrain.Looks.Glyph;
                console.SetCellAppearance(location.X, location.Y, unseenCell);
            }

            UpdateViewOfCoords(console, FOV.NewlySeen);
        }

        public void UpdateViewOfCoords(ScrollingConsole console, IEnumerable<Coord> coords)
        {
            //  Show the BG color of the terrain, then in order:
            //  The FG of any visible being at the location, or
            //  the FG of the topmost item at the loc'n, or
            //  the FG of the terrain type.
            //  Then (later, 0.5?) modified by lighting, et c.
            foreach (var location in coords)
            {
                var targetCell = new Cell();
                var rawCell = SpaceMap.GetItem(location).Terrain.Looks;
                targetCell.Background = rawCell.Background;

                var beings = BeingMap.GetItems(location).ToList();
                var items = ItemMap.GetItems(location).ToList();
                if (beings.Any())
                {
                    var being = beings.Last();
                    targetCell.Foreground = being.Foreground;
                    targetCell.Glyph = being.Glyph;
                }
                else if (items.Any())
                {
                    var item = items.Last();
                    targetCell.Foreground = item.Foreground;
                    targetCell.Glyph = item.Glyph;
                }
                else
                {
                    targetCell.Foreground = rawCell.Foreground;
                    targetCell.Glyph = rawCell.Glyph;
                }

                console.SetCellAppearance(location.X, location.Y, targetCell);
            }

        }
    }
}
