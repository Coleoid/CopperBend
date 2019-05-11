using System;
using System.Collections.Generic;
using GoRogue;
using GoRogue.MapViews;
using CopperBend.Contract;
using CopperBend.Model;

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

        public bool CanPlant(Coord location)
        {
            throw new NotImplementedException();
        }

        public bool CanSeeThrough(Coord location)
        {
            throw new NotImplementedException();
        }

        public bool CanWalkThrough(Coord location)
        {
            throw new NotImplementedException();
        }

        public IMapView<int> GetView_BlightStrength()
        {
            throw new NotImplementedException();
        }

        public IMapView<bool> GetView_CanSeeThrough()
        {
            throw new NotImplementedException();
        }

        public IMapView<bool> GetView_CanWalkThrough()
        {
            throw new NotImplementedException();
        }

        public bool IsWithinMap(Coord location)
        {
            throw new NotImplementedException();
        }

        public DisplayBuffer DisplayBuffer { get; internal set; }


        //  We only need to update two categories of cells:
        //  Cells in FOV, and cells leaving FOV.
        //  Also, only those within the scope of the UI.
        //  I hope the UI bounding box can be handled by FOV calc.
        //  It will technically have us updating a border of cells
        //  outside the viewport (the edge moved away from), but I
        //  like the ratio of effort saved to remaining inefficiency.

        //  Start with the 'known' flag of each location
        //  if not known, output blank.
        //  Next, if known but out of Field of View (FOV),
        //  render just terrain type in "map neutral" colors.
        //  The remainder are in FOV:
        //  Show the BG color of the terrain, then in order:
        //  The FG of any visible being at the location, or
        //  the FG of the topmost item at the loc'n, or
        //  the FG of the terrain type.
        //  Then (later, 0.5?) modified by lighting, et c.

        public void UpdateFromFOV(FOV fov, Being player)
        {
            fov.Calculate(player.Position);

            foreach (var location in fov.NewlyUnseen)
            {

            }
        }

        //public void CellsEnterFOV(IEnumerable<Coord> newlySeen)
        //{
        //    throw new NotImplementedException();
        //}

        //public void CellsLeaveFOV(IEnumerable<Coord> newlyUnseen)
        //{
        //    //  for each location, if ever seen,
        //    //  write the FG of the Terrain of these cells in
        //    //  map neutral colors (dark gray?  scroll + sepia?)
        //    throw new NotImplementedException();
        //}

    }
}
