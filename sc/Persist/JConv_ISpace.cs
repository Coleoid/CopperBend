using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CopperBend.Contract;
using CopperBend.Fabric;

namespace CopperBend.Persist
{
    public class JConv_ISpace : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanConvert(Type objectType) => objectType is ISpace;

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
            //if (!Debugger.IsAttached) Debugger.Launch();
            return ReadJObject(jOb, serializer);
        }

        public ISpace ReadJObject(JObject jOb, JsonSerializer serializer)
        {
            ISpace space = new Space();
            serializer.Populate(jOb.CreateReader(), space);

            return space;
        }
    }
}
