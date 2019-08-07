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
    public class ItemMap : MultiSpatialMap<IItem>
    {
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
            ItemMap map = new ItemMap();

            //var jMap = JObject.Load(reader);
            //var jItems = jMap["Items"];
            //var items = JArray.Load(jItems.CreateReader());
            var items = JArray.Load(reader);

            var itemConverter = new Converter_of_IItem();
            foreach (JObject jOb in items)
            {
                var item = itemConverter.ReadJObject(jOb["Item"] as JObject, serializer);
                map.Add(item, item.Location);
            }
            
            //serializer.Populate(reader, map);
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
            var item = default(IItem);
            switch (jOb["ItemType"].Value<string>())
            {
            case "Item":
                item = new Item((0, 0));
                break;
            case "Knife":
                item = new Knife((0, 0));
                break;
            }
            serializer.Populate(jOb.CreateReader(), item);

            var locn = jOb["Location"].ToString();
            var nums = Regex.Matches(locn, @"\d+");
            int x = int.Parse(nums[1].Value);
            int y = int.Parse(nums[2].Value);
            item.Location = new Coord(x, y);

            return item;
        }
    }
}
