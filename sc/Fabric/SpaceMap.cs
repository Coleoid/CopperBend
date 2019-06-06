using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Model;

namespace CopperBend.Fabric
{
    public class SpaceMap
    {
        public static Dictionary<string, TerrainType> TerrainTypes { get; internal set; }
        public static TerrainType TilledSoil => TerrainTypes["tilled dirt"];
        public static TerrainType PlantedSoil => TerrainTypes["planted dirt"];

        public int Width { get; set; }
        public int Height { get; set; }
        public Coord PlayerStartPoint { get; set; }

        public Dictionary<string, Space> AllSpaces { get; set; } = new Dictionary<string, Space>();

        [JsonIgnore]
        public SpatialMap<Space> Spatial { get; set; }

        public SpaceMap(int width, int height)
        {
            Width = width;
            Height = height;
            Spatial = new SpatialMap<Space>(width * height);
        }

        public Space GetSpace(Coord position)
        {
            SyncSpaces();
            return Spatial.GetItem(position);
        }

        public void AddSpace(Space space, Coord position)
        {
            SyncSpaces();
            Spatial.Add(space, position);
            AllSpaces[position.ToString()] = space;
        }

        /// <summary> Populates the SpatialMap after deserializing </summary>
        /// <remarks> Honestly, I'm not the biggest fan of this. </remarks>
        public void SyncSpaces()
        {
            if (Spatial.Any() || !AllSpaces.Any()) return;

            foreach (string coordString in AllSpaces.Keys)
            {
                var nums = Regex.Matches(coordString, @"\d+");
                int x = int.Parse(nums[0].Value);
                int y = int.Parse(nums[1].Value);
                Coord coord = new Coord(x, y);
                Spatial.Add(AllSpaces[coordString], coord);
            }
        }

        public bool CanWalkThrough(Coord position)
        {
            // off the map is not walkable
            if (position.X < 0 || position.X >= Width
             || position.Y < 0 || position.Y >= Height)
                return false;

            return GetSpace(position).CanWalkThrough;
        }

        public bool CanSeeThrough(Coord position)
        {
            // off the map is not visible
            if (position.X < 0 || position.X >= Width
             || position.Y < 0 || position.Y >= Height)
                return false;

            return GetSpace(position).CanSeeThrough;
        }

        public bool CanPlant(Coord position)
        {
            // off the map is not plantable
            if (position.X < 0 || position.X >= Width
             || position.Y < 0 || position.Y >= Height)
                return false;

            return GetSpace(position).CanPlant;
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
                GetSpace(seen).IsKnown = true;
            }
        }

        internal bool OpenDoor(Space tile)
        {
            if (tile.Terrain == TerrainTypes["closed door"])
            {
                tile.Terrain = TerrainTypes["open door"];
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

        [YamlIgnore]
        [JsonIgnore]
        //0.2.MAP  check for modifiers (smoke, dust, giant creature, ...)
        public bool CanSeeThrough => Terrain.CanSeeThrough;
        [YamlIgnore]
        [JsonIgnore]
        public bool CanWalkThrough => Terrain.CanWalkThrough;

        [YamlIgnore]
        [JsonIgnore]
        //0.2.MAP  check for modifiers (permission, hostile aura, blight, ...)
        public bool CanPlant => Terrain.CanPlant && IsTilled && !IsSown;
        [YamlIgnore]
        [JsonIgnore]
        public bool CanTill => Terrain.CanPlant && !IsTilled;

        public bool IsTilled { get; set; }
        public bool IsSown { get; set; }
        public bool IsKnown { get; set; }
    }

    public class AreaBlight : IHasID, IDestroyable
    {
        public AreaBlight(uint id = uint.MaxValue)
        {
            ID = (id == uint.MaxValue ? IDGenerator.UseID() : id);
        }

        #region standard IHasID
        public static IDGenerator IDGenerator;
        public uint ID { get; private set; }
        #endregion

        public int Extent { get; set; }

        #region IDestroyable
        public int MaxHealth => 80;

        public int Health => Extent;

        public void Heal(int amount)
        {
            Guard.Against(amount < 0, $"Cannot heal negative amount {amount}.");
            Extent = Math.Min(MaxHealth, Extent + amount);
        }

        public void Hurt(int amount)
        {
            Guard.Against(amount < 0, $"Cannot hurt negative amount {amount}.");
            Extent = Math.Max(0, Extent - amount);
        }
        #endregion
    }

}
