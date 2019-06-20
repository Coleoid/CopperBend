using GoRogue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CopperBend.Fabric
{
    public class SerializableMultiSpatialMap<T> where T : class, IHasID
    {
        [JsonIgnore]
        public MultiSpatialMap<T> SpatialMap { get; set; }

        /// <summary> The serialization border for our mapped items.  Shouldn't be used by normal app code. </summary>
        /// <remarks> Would be a touch nicer with a string -> Coord implicit conversion. </remarks>
        public Dictionary<string, List<T>> SerialItems
        {
            get
            {
                var serialItems = new Dictionary<string, List<T>>();
                foreach (var coord in SpatialMap.Positions)
                {
                    var items = SpatialMap.GetItems(coord);
                    serialItems.Add(coord.ToString(), items.ToList());
                }

                return serialItems;
            }

            set
            {
                SpatialMap = new MultiSpatialMap<T>();
                foreach (var key in value.Keys)
                {
                    var nums = Regex.Matches(key, @"\d+");
                    int x = int.Parse(nums[0].Value);
                    int y = int.Parse(nums[1].Value);
                    Coord coord = new Coord(x, y);
                    foreach (var entry in value[key])
                    {
                        SpatialMap.Add(entry, coord);
                    }
                }
            }
        }


        public SerializableMultiSpatialMap(int initialCapacity = 32)
        {
            SpatialMap = new MultiSpatialMap<T>(initialCapacity);
        }

        public IEnumerable<T> GetItems(Coord position)
        {
            IEnumerable<T> items = SpatialMap.GetItems(position);
            return items;
        }

        public void AddItem(T item, Coord position)
        {
            SpatialMap.Add(item, position);
        }

        public bool RemoveItem(T item)
        {
            return SpatialMap.Remove(item);
        }
    }

}
