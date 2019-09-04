using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Model;
using Microsoft.Xna.Framework;

namespace CopperBend.Persist
{
    public class JConv_IBeing : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanConvert(Type objectType) => objectType is IBeing;

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

        public IBeing ReadJObject(JObject jOb, JsonSerializer serializer)
        {
            var fg = jOb["Foreground"].ToObject<Color>();
            var bg = jOb["Background"].ToObject<Color>();
            var glyph = jOb["Glyph"].ToObject<char>();
            var id = jOb["ID"].ToObject<uint>();

            IBeing being;
            var beingType = jOb["BeingType"].Value<string>();
            switch (beingType)
            {
            case "Being":
                //throw new Exception("Is it ever legit to do this?");
                being = new Being(fg, bg, glyph, id);
                break;
            case "Player":
                being = new Player(fg, bg, glyph, id);
                break;
            case "Monster":
                being = new Monster(fg, bg, glyph, id);
                break;

            default:
                throw new Exception($"Unknown being type [{beingType}].");
            }

            serializer.Populate(jOb.CreateReader(), being);

            return being;
        }
    }

    }
