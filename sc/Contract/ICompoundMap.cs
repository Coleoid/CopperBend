using System;
using System.Collections.Generic;
using CopperBend.Engine;
using CopperBend.Model;
using GoRogue;
using GoRogue.MapViews;
using Microsoft.Xna.Framework;
using SadConsole;
using Rectangle = GoRogue.Rectangle;

namespace CopperBend.Contract
{
    public interface ICompoundMap
    {
        int Width { get; }
        int Height { get; }
        bool IsWithinMap(Point point);

        SpaceMap SpaceMap { get; }
        MultiSpatialMap<IBeing> BeingMap { get; }
        MultiSpatialMap<IItem> ItemMap { get; }
        SpatialMap<AreaBlight> BlightMap { get; }

        List<LocatedTrigger> LocatedTriggers { get; }

        /// <summary> Can Plant considering terrain, blight, existing plants, and ownership. </summary>
        bool CanPlant(Point point);

        bool CanSeeThrough(Point point);
        bool CanWalkThrough(Point point);
        IMapView<bool> GetView_CanSeeThrough();
        IMapView<bool> GetView_CanWalkThrough();
        IMapView<int> GetView_BlightStrength();
    }

    public class CompoundMap : ICompoundMap
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public SpaceMap SpaceMap { get; set; }

        public MultiSpatialMap<IBeing> BeingMap { get; set; }

        public MultiSpatialMap<IItem> ItemMap { get; set; }

        public SpatialMap<AreaBlight> BlightMap { get; set; }

        public List<LocatedTrigger> LocatedTriggers { get; set; }

        public bool CanPlant(Point point)
        {
            throw new NotImplementedException();
        }

        public bool CanSeeThrough(Point point)
        {
            throw new NotImplementedException();
        }

        public bool CanWalkThrough(Point point)
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

        public bool IsWithinMap(Point point)
        {
            throw new NotImplementedException();
        }
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


    public class Region
    {
        public Region()
        {
            Areas = new List<Rectangle>();
        }

        public List<Rectangle> Areas { get; }

        public void AddArea(Rectangle area)
        {

        }

        public bool Contains(Point point)
        {
            foreach (var area in Areas)
            {
                if (area.Contains(point)) return true;
            }

            return false;
        }
    }

    public class BlightRegion : Region
    {

        public int Strength { get; set; }
    }

    public class LocatedTrigger
    {
        /// <summary> Whatever moved into the </summary>
        public Func<IHasID, bool> Trigger { get; }
        public Region Region { get; }
    }

}
