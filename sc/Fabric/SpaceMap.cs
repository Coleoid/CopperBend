using System;
using System.Collections.Generic;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Model;  //0.1: later, Seed => ISowable, as the model matures

namespace CopperBend.Fabric
{
    public class SpaceMap : SpatialMap<Space>
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Coord PlayerStartPoint { get; set; }
        public static Dictionary<string, TerrainType> TerrainTypes { get; internal set; }
        public static TerrainType TilledSoil => TerrainTypes["tilled dirt"];

        public SpaceMap(int width, int height)
            : base(height * width)
        {
            Width = width;
            Height = height;
        }

        public bool CanWalkThrough(Coord position)
        {
            // off the map is not walkable
            if (position.X < 0 || position.X >= Width
             || position.Y < 0 || position.Y >= Height)
                return false;

            return GetItem(position).CanWalkThrough;
        }

        public bool CanSeeThrough(Coord position)
        {
            // off the map is not visible
            if (position.X < 0 || position.X >= Width
             || position.Y < 0 || position.Y >= Height)
                return false;

            return GetItem(position).CanSeeThrough;
        }

        public bool CanPlant(Coord position)
        {
            // off the map is not plantable
            if (position.X < 0 || position.X >= Width
             || position.Y < 0 || position.Y >= Height)
                return false;

            return GetItem(position).CanPlant;
        }


        internal void Sow(Space space, Seed seedToSow)
        {
            throw new NotImplementedException();
        }

        internal void Till(Space space)
        {
            if (space.CanTill && !space.IsTilled)
            {
                space.Terrain = TilledSoil;
            }
        }

        internal void SeeCoords(IEnumerable<Coord> newlySeen)
        {
            foreach (var seen in newlySeen)
            {
                GetItem(seen).IsKnown = true;
            }
        }
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
        public bool CanPlant => Terrain.CanPlant && IsTilled;
        public bool CanTill => Terrain.CanPlant && !IsTilled;

        public bool IsTilled { get; internal set; }
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

}
