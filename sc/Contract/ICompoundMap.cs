using System;
using System.Collections.Generic;
using SadConsole;
using GoRogue;
using GoRogue.MapViews;
using Rectangle = GoRogue.Rectangle;
using CopperBend.Fabric;
using CopperBend.Model;

namespace CopperBend.Contract
{
    public interface ICompoundMap
    {
        int Width { get; }
        int Height { get; }
        bool IsWithinMap(Coord position);

        SpaceMap SpaceMap { get; }
        MultiSpatialMap<IBeing> BeingMap { get; }
        MultiSpatialMap<IItem> ItemMap { get; }
        SpatialMap<AreaBlight> BlightMap { get; }

        List<LocatedTrigger> LocatedTriggers { get; }

        /// <summary> Can Plant considering terrain, blight, existing plants, and ownership. </summary>
        bool CanPlant(Coord position);

        FOV FOV { get; set; }

        bool CanSeeThrough(Coord position);
        bool CanWalkThrough(Coord position);
        IMapView<bool> GetView_CanSeeThrough();
        IMapView<bool> GetView_CanWalkThrough();
        IMapView<int> GetView_BlightStrength();

        void SetInitialConsoleCells(ScrollingConsole console, SpaceMap spaceMap);
        void UpdateFromFOV(ScrollingConsole console, FOV fov, Coord position);
    }

    public class DisplayBuffer
    {
        public Cell[] RawCells { get; internal set; }
        public Cell[] OutputCells { get; internal set; }
    }

    public class TerrainType
    {
        public string Name;
        public Cell Looks; //0.1
        public bool CanSeeThrough;
        public bool CanWalkThrough;
        public bool CanPlant;
    }

    public class Space : IHasID
    {
        #region standard IHasID
        // one IDGenerator for all Spaces
        public static IDGenerator IDGenerator = new IDGenerator();
        public uint ID { get; private set; } = IDGenerator.UseID();
        #endregion

        //public int Elevation;  //for later movement/attack mod
        public TerrainType Terrain;

        //0.2.  0.3 accounts for modifiers (smoke, dust, giant creature, ...)
        public bool CanSeeThrough => Terrain.CanSeeThrough;
        public bool CanWalkThrough => Terrain.CanWalkThrough;

        //0.2.  0.3 accounts for modifiers (permission, hostile aura, blight, ...)
        public bool CanPlant => Terrain.CanPlant;

        public bool CanPreparePlanting { get; internal set; }
        public bool IsTilled { get; internal set; }
        public bool IsTillable { get; internal set; }
        public bool IsSown { get; internal set; }
        public bool IsKnown { get; internal set; }
    }

    public class AreaBlight : IHasID
    {
        #region standard IHasID
        // one IDGenerator for all AreaBlight
        public static IDGenerator IDGenerator = new IDGenerator();
        public uint ID { get; private set; } = IDGenerator.UseID();
        #endregion
        public int Extent { get; set; }
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
