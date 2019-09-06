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

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            IBook book = (IBook)value;

            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

            EmitNextPair("BookType", book.BookType, emitter);

            switch (book.BookType)
            {
            case "TomeOfChaos":
                EmitTome(book, emitter);
                break;

            //0.1.SAVE:  Write remainder of Compendium

            default:
                throw new NotImplementedException($"Not ready to Write book type [{book.BookType}].");
            }

            emitter.Emit(new MappingEnd());
        }

        private void EmitTome(IBook book, IEmitter emitter)
        {
            var tome = (TomeOfChaos)book;

            EmitNextPair("TopSeed", tome.TopSeed, emitter);
            EmitNextPair("TopGenerator", SerializedRNG(tome.TopGenerator), emitter);
            EmitNextPair("MapTopGenerator", SerializedRNG(tome.MapTopGenerator), emitter);
            EmitNextPair("LearnableTopGenerator", SerializedRNG(tome.LearnableTopGenerator), emitter);
            EmitNextPair($"LearnableTopGenerator[{Learnables.Seed}]", SerializedRNG(tome.LearnableRndGen(Learnables.Seed)), emitter);

            //0.1.SAVE:  Write remainder of Tome, these named sets are scaling... iffily.
        }

        public void EmitNextPair(string key, string value, IEmitter emitter)
        {
            emitter.Emit(new Scalar(null, key));
            emitter.Emit(new Scalar(null, value));
        }

        public object ReadYaml(IParser parser, Type type)
        {
            //if (!Debugger.IsAttached) Debugger.Launch();
            IBook book = null;

            parser.Expect<MappingStart>();
            var bookType = getValueNext("BookType", parser);

            switch (bookType)
            {
            case "TomeOfChaos":
                book = ParseTome(parser);
                break;

            //0.1.SAVE:  Read remainder of Compendium

            default:
                throw new NotImplementedException($"Not ready to Read book type [{bookType}].");
            }

            parser.Expect<MappingEnd>();
            return book;
        }

        private TomeOfChaos ParseTome(IParser parser)
        {
            TomeOfChaos tome = null;

            var topSeed = getValueNext("TopSeed", parser);
            tome = new TomeOfChaos(topSeed);

            var rng_b64 = getValueNext("TopGenerator", parser);
            tome.TopGenerator = RngFromBase64(rng_b64);

            //0.1.SAVE: Parse out the remaining RNGs

            return tome;
        }

        private string getValueNext(string valueName, IParser parser)
        {
            ExpectScalar(parser, valueName);
            parser.MoveNext();
            var val = getScalarString(parser);
            parser.MoveNext();
            return val;
        }

        private void ExpectScalar(IParser parser, string expectedString)
        {
            string label = getScalarString(parser);

            if (label != expectedString)
                throw new Exception($"Expected '{expectedString}', got '{label}'.");
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
