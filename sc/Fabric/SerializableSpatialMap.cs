using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using GoRogue;

namespace CopperBend.Fabric
{
    public class SerializableSpatialMap<T> where T: class, IHasID
    {
        public SerializableSpatialMap(int initialCapacity = 32)
        {
            SpatialMap = new SpatialMap<T>(initialCapacity);
        }

        [JsonIgnore]
        public SpatialMap<T> SpatialMap { get; set; }

        /// <summary> The serialization border for our mapped items.
        /// Shouldn't be used by normal app code. </summary>
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
}
