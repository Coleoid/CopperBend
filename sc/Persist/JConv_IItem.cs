using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Model;
using System.Diagnostics;

namespace CopperBend.Persist
{
    public class JConv_IItem : JsonConverter
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
            var id = jOb["ID"].ToObject<uint>();
            var locnText = jOb["Location"].ToString();
            var nums = Regex.Matches(locnText, @"\d+");
            int x = int.Parse(nums[1].Value);
            int y = int.Parse(nums[2].Value);
            var coord = new Coord(x, y);


            var itemType = jOb["ItemType"].Value<string>();
            IItem item = default;
            switch (itemType)
            {
            case "Item":
                throw new Exception("Is it ever legit to do this?");
                //item = new Item(coord, id: id);
                //break;
            case "Knife":
                item = new Knife(coord, id: id);
                break;
            case "Fruit":
                var quantity = jOb["Quantity"].Value<int>();
                var details = jOb["PlantDetails"].ToObject<PlantDetails>();
                item = new Fruit(coord, quantity, details, id);
                break;
            case "Seed":
                //quantity = jOb["Quantity"].Value<int>();
                details = jOb["PlantDetails"].ToObject<PlantDetails>();
                item = new Seed(coord, details, id: id);
                break;
            case "Hoe":
                item = new Hoe(coord, id);
                break;

            default:
                throw new Exception($"Unknown item type [{itemType}].");
            }

            serializer.Populate(jOb.CreateReader(), item);

            return item;
        }
    }
}
