using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace CopperBend.Fabric
{
    public static class Persistence_Extensions
    {
        public static string GetKVP_string(this IParser parser, string valueName)
        {
            string label = parser.GetScalar();
            if (label != valueName)
                throw new Exception($"Expected '{valueName}', got '{label}'.");

            var val = parser.GetScalar();
            return val;
        }

        public static int GetKVP_int(this IParser parser, string valueName)
        {
            return int.Parse(parser.GetKVP_string(valueName), CultureInfo.InvariantCulture);
        }

        public static uint GetKVP_uint(this IParser parser, string valueName)
        {
            return uint.Parse(parser.GetKVP_string(valueName), CultureInfo.InvariantCulture);
        }

        public static bool GetKVP_bool(this IParser parser, string valueName)
        {
            return bool.Parse(parser.GetKVP_string(valueName));
        }

        public static Color GetKVP_Color(this IParser parser, string valueName)
        {
            return Color_FromString(parser.GetKVP_string(valueName));
        }

        public static Color Color_FromString(string text)
        {
            var match = Regex.Match(text, @"R:(\d+) G:(\d+) B:(\d+) A:(\d+)");
            int r = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            int g = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            int b = int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
            int a = int.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
            return new Color(r, g, b, a);
        }

        public static Point GetKVP_Point(this IParser parser, string valueName)
        {
            return Point_FromString(parser.GetKVP_string(valueName));
        }

        public static Point Point_FromString(string text)
        {
            var match = Regex.Match(text, @"X:(\d+) Y:(\d+)");
            int x = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            int y = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            return new Point(x, y);
        }


        public static string GetScalar(this IParser parser)
        {
            parser.TryConsume<Scalar>(out Scalar scalar);
            return scalar.Value;
        }

        public static (string k, string v) GetKVP(this IParser parser)
        {
            string k = parser.GetScalar();
            string v = parser.GetScalar();
            return (k, v);
        }

        public static void EmitKVP(this IEmitter emitter, string key, uint value)
        {
            emitter.EmitKVP(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public static void EmitKVP(this IEmitter emitter, string key, int value)
        {
            emitter.EmitKVP(key, value.ToString(CultureInfo.InvariantCulture));
        }
        public static void EmitKVP(this IEmitter emitter, string key, bool value)
        {
            emitter.EmitKVP(key, value.ToString());
        }

        public static void EmitKVP(this IEmitter emitter, string key, string value)
        {
            emitter.Emit(new Scalar(null, key));
            emitter.Emit(new Scalar(null, value));
        }

        public static void StartNamedMapping(this IEmitter emitter, string name)
        {
            emitter.Emit(new Scalar(name));
            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));
        }

        public static void EndMapping(this IEmitter emitter)
        {
            emitter.Emit(new MappingEnd());
        }
    }
}
