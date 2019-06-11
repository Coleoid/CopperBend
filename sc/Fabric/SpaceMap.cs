using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Model;

namespace CopperBend.Fabric
{
    public class SerializableSpatialMap<T> where T: class, IHasID
    {
        [JsonIgnore]
        public SpatialMap<T> SpatialMap { get; set; }

        /// <summary> The serialization border for our mapped items.  Shouldn't be used by normal app code. </summary>
        /// <remarks> Would be a touch nicer with a string -> Coord implicit conversion. </remarks>
        public Dictionary<string, T> SerialItems
        {
            get
            {
                var items = new Dictionary<string, T>();
                foreach (var coord in SpatialMap.Positions)
                {
                    var item = SpatialMap.GetItem(coord);
                    items.Add(coord.ToString(), item);
                }

                return items;
            }

            set
            {
                SpatialMap = new SpatialMap<T>();
                foreach (var key in value.Keys)
                {
                    var nums = Regex.Matches(key, @"\d+");
                    int x = int.Parse(nums[0].Value);
                    int y = int.Parse(nums[1].Value);
                    Coord coord = new Coord(x, y);
                    SpatialMap.Add(value[key], coord);
                }
            }
        }


        public SerializableSpatialMap(int initialCapacity = 32)
        {
            SpatialMap = new SpatialMap<T>(initialCapacity);
        }

        public T GetItem(Coord position)
        {
            T item = SpatialMap.GetItem(position);
            return item;
        }

        public void AddItem(T item, Coord position)
        {
            SpatialMap.Add(item, position);
        }

        public void RemoveItem(T item)
        {
            SpatialMap.Remove(item);
        }
    }

    public class BlightMap : SerializableSpatialMap<AreaBlight>
    {
        public string Name { get; set; }

        //0.0  the arguments sidestep a Json.net deserializing bug:
        //  it won't reload SerialItems when creating with default ctor
        public BlightMap(int width, int height)
            : base()
        {
        }
        
        //public BlightMap()
        //    : base()
        //{
        //}
    }

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
        public int MaxHealth { get; set; } = 80;

        [JsonIgnore]
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
