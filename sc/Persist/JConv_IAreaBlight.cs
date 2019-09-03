using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CopperBend.Contract;
using CopperBend.Fabric;

namespace CopperBend.Persist
{
    public class JConv_IAreaBlight : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanConvert(Type objectType) => objectType is IAreaBlight;

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

        public IAreaBlight ReadJObject(JObject jOb, JsonSerializer serializer)
        {
            var id = jOb["ID"].ToObject<uint>();
            IAreaBlight blight = new AreaBlight(id);
            serializer.Populate(jOb.CreateReader(), blight);
            return blight;
        }
    }
}
