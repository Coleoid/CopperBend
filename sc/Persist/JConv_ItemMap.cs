using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CopperBend.Fabric;

namespace CopperBend.Persist
{
    public class JConv_ItemMap : JsonConverter
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
            var itemConverter = new JConv_IItem();
            ItemMap map = new ItemMap();

            foreach (JObject jOb in JArray.Load(reader))
            {
                var item = itemConverter.ReadJObject(jOb["Item"] as JObject, serializer);
                map.Add(item, item.Location);
            }

            return map;
        }
    }
}
