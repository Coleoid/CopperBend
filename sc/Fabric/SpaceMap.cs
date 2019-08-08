using System.Collections.Generic;
using Newtonsoft.Json;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Model;

namespace CopperBend.Fabric
{
    public class SpaceMap : SerializableSpatialMap<Space>
    {
        public static Dictionary<string, TerrainType> TerrainTypes { get; internal set; }
        public static TerrainType TilledSoil => TerrainTypes["tilled dirt"];
        public static TerrainType PlantedSoil => TerrainTypes["planted dirt"];
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


        internal void Sow(Space space, Seed seedToSow)
        {
            if (space.CanPlant)
            {
                space.Terrain = PlantedSoil;
                space.IsSown = true;
            }
        }

        internal void Till(Space space)
        {
            if (space.CanTill)
            {
                space.Terrain = TilledSoil;
                space.IsTilled = true;
            }
        }

        internal void SeeCoords(IEnumerable<Coord> newlySeen)
        {
            foreach (var seen in newlySeen)
            {
                GetItem(seen).IsKnown = true;
            }
        }

        internal bool OpenDoor(Space space)
        {
            if (space.Terrain == TerrainTypes["closed door"])
            {
                space.Terrain = TerrainTypes["open door"];
                return true;
            }

            return false;
        }
    }

    public class Space : IHasID
    {
        public Space(uint id = uint.MaxValue)
        {
            ID = (id == uint.MaxValue ? IDGenerator.UseID() : id);
        }

        #region standard IHasID
        public static IDGenerator IDGenerator;
        public uint ID { get; private set; }
        #endregion

        //public int Elevation;  //for later movement/attack mod
        public TerrainType Terrain;

        [JsonIgnore]
        //0.2.MAP  check for modifiers (smoke, dust, giant creature, ...)
        public bool CanSeeThrough => Terrain.CanSeeThrough;
        [JsonIgnore]
        public bool CanWalkThrough => Terrain.CanWalkThrough;

        [JsonIgnore]
        //0.2.MAP  check for modifiers (permission, hostile aura, blight, ...)
        public bool CanPlant => Terrain.CanPlant && IsTilled && !IsSown;
        [JsonIgnore]
        public bool CanTill => Terrain.CanPlant && !IsTilled;

        public bool IsTilled { get; set; }
        public bool IsSown { get; set; }
        public bool IsKnown { get; set; }
    }
}
