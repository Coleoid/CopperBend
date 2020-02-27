using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Troschuetz.Random.Generators;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace CopperBend.Persist
{
    public class Persistence_util
    {

        protected string GetValueNext(IParser parser, string valueName)
        {
            string label = GetScalar(parser);
            if (label != valueName)
                throw new Exception($"Expected '{valueName}', got '{label}'.");

            var val = GetScalar(parser);
            return val;
        }

        protected string GetScalar(IParser parser)
        {
            parser.TryConsume<Scalar>(out Scalar scalar);
            return scalar.Value;
        }

        public void EmitKVP(IEmitter emitter, string key, string value)
        {
            emitter.Emit(new Scalar(null, key));
            emitter.Emit(new Scalar(null, value));
        }

        public Color Color_FromString(string text)
        {
            var match = Regex.Match(text, @"R:(\d+) G:(\d+) B:(\d+) A:(\d+)");
            int r = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            int g = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            int b = int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
            int a = int.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
            return new Color(r, g, b, a);
        }

        public Point Point_FromString(string text)
        {
            var match = Regex.Match(text, @"X:(\d+) Y:(\d+)");
            int x = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            int y = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            return new Point(x, y);
        }

        public AbstractGenerator RngFromBase64(string rng_b64)
        {
            byte[] rng_bytes = Convert.FromBase64String(rng_b64);
            var rng = (NR3Generator)ByteArrayToObject(rng_bytes);
            return rng;
        }

        public static object ByteArrayToObject(byte[] arrBytes)
        {
            using var memStream = new MemoryStream(arrBytes);
            var binForm = new BinaryFormatter();
            var obj = binForm.Deserialize(memStream);
            return obj;
        }

        public string SerializedRNG(AbstractGenerator generator)
        {
            byte[] gen_bytes = ObjectToByteArray(generator);
            string gen_b64 = Convert.ToBase64String(gen_bytes);
            return gen_b64;
        }

        public static byte[] ObjectToByteArray(object target)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using var ms = new MemoryStream();
            bf.Serialize(ms, target);
            return ms.ToArray();
        }

    }
}
