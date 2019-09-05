using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CopperBend.Contract;
using CopperBend.Model;
using CopperBend.Fabric;

namespace CopperBend.Persist
{
    public class JConv_IBook : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanConvert(Type objectType) => objectType is IBook;

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

        public IBook ReadJObject(JObject jOb, JsonSerializer serializer)
        {
            string distinguisher = "BookType";
            if (!jOb.HasValues || jOb[distinguisher] == null)
                throw new Exception($"Unexpectedly missing a distinguisher named {distinguisher} in JObject:\n{jOb.ToString()}");

            var bookType = jOb[distinguisher].Value<string>();
            IBook book;
            switch (bookType)
            {
            case "TomeOfChaos":
                book = new TomeOfChaos();
                break;

            default:
                throw new Exception($"Don't know how to create book type [{bookType}].");
            }

            serializer.Populate(jOb.CreateReader(), book);

            return book;
        }
    }
}
