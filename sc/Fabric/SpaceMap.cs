using System.Collections.Generic;
using GoRogue;
using CopperBend.Contract;

namespace CopperBend.Fabric
{
    public class SpaceMap : SpatialMap<ISpace>, ISpaceMap
    {
        public static Dictionary<string, Terrain> TerrainTypes { get; } = new Dictionary<string, Terrain>();
        public static Terrain TilledSoil => TerrainTypes[TerrainEnum.SoilTilled];
        public static Terrain PlantedSoil => TerrainTypes[TerrainEnum.SoilPlanted];
        public Coord PlayerStartPoint { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public SpaceMap(int width, int height)
        : base(width * height)
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

        //1.+: Change these to Guards?
        public void MarkSpaceSown(ISpace space)
        {
            if (space.CanPlant)
            {
                space.Terrain = PlantedSoil;
                space.IsSown = true;
            }
        }

        public void Till(ISpace space)
        {
            if (space.CanTill)
            {
                space.Terrain = TilledSoil;
                space.IsTilled = true;
            }
        }

        public void SeeCoords(IEnumerable<Coord> newlySeen)
        {
            foreach (var seen in newlySeen)
            {
                GetItem(seen).IsKnown = true;
            }
        }

        public bool OpenDoor(ISpace space)
        {
            if (space.Terrain == TerrainTypes["closed door"])
            {
                space.Terrain = TerrainTypes["open door"];
                return true;
            }

            return false;
        }
    }

    public class Space : IHasID, ISpace
    {
        public Space(uint id = uint.MaxValue)
        {
            ID = (id == uint.MaxValue ? IDGenerator.UseID() : id);
        }

        #region My IHasID
        public static void SetIDGenerator(IDGenerator generator)
        {
            IDGenerator = generator;
        }
        private static IDGenerator IDGenerator { get; set; }
        public uint ID { get; private set; }
        #endregion

        //public int Elevation;  //for later movement/attack mod
        public Terrain Terrain { get; set; }

        //0.2.MAP  check for modifiers (smoke, dust, giant creature, ...)
        public bool CanSeeThrough => Terrain.CanSeeThrough;
        public bool CanWalkThrough => Terrain.CanWalkThrough;

        //0.2.MAP  check for modifiers (permission, hostile aura, rot, ...)
        public bool CanPlant => Terrain.CanPlant && IsTilled && !IsSown;
        public bool CanTill => Terrain.CanPlant && !IsTilled;

        public bool IsTilled { get; set; }
        public bool IsSown { get; set; }
        public bool IsKnown { get; set; }
    }
}
