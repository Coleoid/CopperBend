using System;
using System.Collections.Generic;
using SadConsole;
using GoRogue;
using GoRogue.MapViews;
using SadConsole.Effects;

namespace CopperBend.Contract
{
    public interface ICompoundMap
    {
        int Width { get; }
        int Height { get; }
        bool IsWithinMap(Coord position);

        ISpaceMap SpaceMap { get; }
        MultiSpatialMap<IBeing> BeingMap { get; }
        IItemMap ItemMap { get; }
        IBlightMap BlightMap { get; }

        List<LocatedTrigger> LocatedTriggers { get; }


        FOV FOV { get; set; }
        /// <summary> When a space changes its CanSeeThrough, set this flag true </summary>
        bool VisibilityChanged { get; set; }
        /// <summary> When a space's appearance (may) change, add its coords to this list </summary>
        List<Coord> CoordsWithChanges { get; }

        bool CanSeeThrough(Coord position);
        bool CanWalkThrough(Coord position);
        /// <summary> Can Plant considering terrain, blight, existing plants, and ownership. </summary>
        bool CanPlant(Coord position);

        IMapView<bool> GetView_CanSeeThrough();
        IMapView<bool> GetView_CanWalkThrough();
        IMapView<int> GetView_BlightStrength();

        EffectsManager EffectsManager { get; }

        void SetInitialConsoleCells(ScrollingConsole console, ISpaceMap spaceMap);
        void UpdateFOV(ScrollingConsole console, Coord position);
        void UpdateViewOfCoords(ScrollingConsole console, IEnumerable<Coord> coords);
    }

    public class TerrainType
    {
        public string Name;
        public Cell Looks; // name for bleh
        public bool CanSeeThrough;
        public bool CanWalkThrough;
        public bool CanPlant;
    }

    //public class Region
    //{
    //    public Region()
    //    {
    //        Areas = new List<Rectangle>();
    //    }

    //    public List<Rectangle> Areas { get; }

    //    public void AddArea(Rectangle area)
    //    {

    //    }

    //    public bool Contains(Coord position)
    //    {
    //        foreach (var area in Areas)
    //        {
    //            if (area.Contains(position)) return true;
    //        }

    //        return false;
    //    }
    //}

    //public class BlightRegion : Region
    //{

    //    public int Strength { get; set; }
    //}

    public class LocatedTrigger
    {
        /// <summary> Whatever moved into the </summary>
        public Func<IHasID, bool> Trigger { get; }
        //public Region Region { get; }
    }

}
