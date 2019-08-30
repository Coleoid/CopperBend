using GoRogue;
using CopperBend.Contract;
using Newtonsoft.Json;
using System;
using CopperBend.Model;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace CopperBend.Fabric
{
    [JsonConverter(typeof(ItemMapConverter))]
    public class ItemMap : MultiSpatialMap<IItem>, IItemMap
    {
        // Experiment.  The default serialization doesn't save properties
        // added to this class, probably because it derives from IEnumerable.
        public string MyName { get; set; }
    }

    public class ItemMapConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ItemMap);
        }
        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var itemConverter = new Converter_of_IItem();
            ItemMap map = new ItemMap();

            foreach (JObject jOb in JArray.Load(reader))
            {
                var item = itemConverter.ReadJObject(jOb["Item"] as JObject, serializer);
                map.Add(item, item.Location);
            }
            
            return map;
        }
    }

    public class Converter_of_IItem : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanConvert(Type objectType) => objectType is IItem;
        
        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var jOb = JObject.Load(reader);
            return ReadJObject(jOb, serializer);
        }

        public IItem ReadJObject(JObject jOb, JsonSerializer serializer)
        {
            var locnText = jOb["Location"].ToString();
            var nums = Regex.Matches(locnText, @"\d+");
            int x = int.Parse(nums[1].Value);
            int y = int.Parse(nums[2].Value);
            var coord = new Coord(x, y);

            var itemType = jOb["ItemType"].Value<string>();
            IItem item = default(IItem);
            switch (itemType)
            {
            case "Item":
                item = new Item(coord);
                break;
            case "Knife":
                item = new Knife(coord);
                break;

            default:
                throw new Exception($"Unknown item type [{itemType}].");
            }
            serializer.Populate(jOb.CreateReader(), item);

            return item;
        }
    }
}
