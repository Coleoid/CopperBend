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

        internal void SeeCoords(IEnumerable<Coord> newlySeen)
        {
            foreach (var seen in newlySeen)
            {
                GetItem(seen).IsKnown = true;
            }
        }
    }
}
