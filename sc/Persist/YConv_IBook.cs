using System;
using System.Diagnostics;
using Troschuetz.Random.Generators;
using YamlDotNet.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using CopperBend.Contract;
using CopperBend.Fabric;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace CopperBend.Persist
{
    public class YConv_IBook : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return typeof(IBook).IsAssignableFrom(type);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            if (!Debugger.IsAttached) Debugger.Launch();
            TomeOfChaos tome = null;
            string bookType = string.Empty;

            parser.Accept<MappingStart>();
            while (parser.MoveNext() && parser.Current.GetType() == typeof(Scalar))
            {
                string label = getScalarString(parser);
                parser.MoveNext();
                switch (label)
                {
                case "BookType":
                    bookType = getScalarString(parser);
                    break;

                case "TopSeed":
                    tome = new TomeOfChaos(getScalarString(parser));
                    break;

                case "TopGenerator":
                    string rng_b64 = getScalarString(parser);
                    tome.TopGenerator = RngFromBase64(rng_b64);
                    break;

                default:
                    throw new NotImplementedException($"Not ready for property {label}.");
                }
            }

            parser.Expect<MappingEnd>();
            return tome;
        }

        private AbstractGenerator RngFromBase64(string rng_b64)
        {
            byte[] rng_bytes = Convert.FromBase64String(rng_b64);
            var rng = (XorShift128Generator)ByteArrayToObject(rng_bytes);
            return rng;
        }

        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream(arrBytes))
            {
                var binForm = new BinaryFormatter();
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }

        private string getScalarString(IParser parser)
        {
            parser.Accept<Scalar>();
            var scalar = parser.Current as Scalar;

            return scalar.Value;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            IBook book = (IBook) value;

            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));
            emitter.Emit(new Scalar(null, "BookType"));
            emitter.Emit(new Scalar(null, book.BookType));

            if (book.BookType == "TomeOfChaos")
            {
                var tome = (TomeOfChaos)book;

                emitter.Emit(new Scalar(null, "TopSeed"));
                emitter.Emit(new Scalar(null, tome.TopSeed));

                emitter.Emit(new Scalar(null, "TopGenerator"));
                emitter.Emit(new Scalar(null, SerializedRNG(tome.TopGenerator)));
            }

            emitter.Emit(new MappingEnd());
        }

        public string SerializedRNG(AbstractGenerator generator)
        {
            byte[] gen_bytes = ObjectToByteArray(generator);
            string gen_b64 = Convert.ToBase64String(gen_bytes);
            return gen_b64;
        }

        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
