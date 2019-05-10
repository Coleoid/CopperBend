using GoRogue;
using CopperBend.Contract;
using CopperBend.Model;
using System;
using System.Collections.Generic;

namespace CopperBend.Engine
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

        public bool CanWalkThrough(Coord location)
        {
            // off the map is not walkable
            if (location.X < 0 || location.X >= Width
             || location.Y < 0 || location.Y >= Height)
                return false;

            return GetItem(location).CanWalkThrough;
        }

        internal void Sow(Space space, Seed seedToSow)
        {
            throw new NotImplementedException();
        }

        internal void Till(Space space)
        {
            if (space.IsTillable && !space.IsTilled)
            {
                space.Terrain = TilledSoil;
            }
        }
    }
}
