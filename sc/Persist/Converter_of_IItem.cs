using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Model;

//  This namespace encapsulates a problematic loop in the dependency graph.
//  .Persist refers to .Model innately, it's creating domain objects.
//  .Contract refers to .Persist, via the [JsonConverter(typeof(Converter_of_IItem))] on IItem.
//  ...but .Contract should not refer to .Model, and now it does, transitively.
//  There's at least one alternate way to connect the JsonConverter...
//      "...and I had to find out WHO HE WAS."
namespace CopperBend.Persist
{
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
