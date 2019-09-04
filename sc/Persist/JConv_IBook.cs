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
            var bookType = jOb["BookType"].Value<string>();
            IBook book;
            switch (bookType)
            {
            case "TomeOfChaos":
                book = new TomeOfChaos();
                break;

            default:
                throw new Exception($"Unknown book type [{bookType}].");
            }

            serializer.Populate(jOb.CreateReader(), book);

            return book;
        }
    }
}
