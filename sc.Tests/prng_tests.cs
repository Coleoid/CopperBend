using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Troschuetz.Random.Generators;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace sc_tests
{
    [TestFixture]
    public class prng_tests
    {
        //  not legit in dontet core
        //[Test]
        //public void Serializing_dotnet_Random_keeps_sequence()
        //{
        //    var r1 = new Random();
        //    for (var i = 0; i < 10; i++)
        //    {
        //        int toss = r1.Next();
        //    }

        //    byte[] ser_r1 = ObjectToByteArray(r1);
        //    string b64_r1 = Convert.ToBase64String(ser_r1);

        //    byte[] ser_r2 = Convert.FromBase64String(b64_r1);
        //    Random r2 = (Random)ByteArrayToObject(ser_r2);

        //    for (var j = 0; j < 100; j++)
        //    {
        //        int r1_out = r1.Next();
        //        int r2_out = r2.Next();

        //        Assert.That(r1_out, Is.EqualTo(r2_out));
        //    }
        //}

        [Test]
        public void Serializing_XorShift128Generator_keeps_sequence()
        {
            var r1 = new XorShift128Generator();
            for (var i = 0; i < 10; i++)
            {
                int toss = r1.Next();
            }

            byte[] ser_r1 = ObjectToByteArray(r1);
            string b64_r1 = Convert.ToBase64String(ser_r1);

            byte[] ser_r2 = Convert.FromBase64String(b64_r1);
            var r2 = (XorShift128Generator)ByteArrayToObject(ser_r2);

            for (var j = 0; j < 100; j++)
            {
                int r1_out = r1.Next();
                int r2_out = r2.Next();

                Assert.That(r1_out, Is.EqualTo(r2_out));
            }
        }


        [Test]
        public void Serializing_MersenneTwister_keeps_sequence()
        {
            var r1 = new MT19937Generator();
            for (var i = 0; i < 10; i++)
            {
                int toss = r1.Next();
            }

            byte[] ser_r1 = ObjectToByteArray(r1);
            string b64_r1 = Convert.ToBase64String(ser_r1);

            byte[] ser_r2 = Convert.FromBase64String(b64_r1);
            var r2 = (MT19937Generator)ByteArrayToObject(ser_r2);

            for (var j = 0; j < 100; j++)
            {
                int r1_out = r1.Next();
                int r2_out = r2.Next();

                Assert.That(r1_out, Is.EqualTo(r2_out));
            }
        }

        [Test]
        public void Serializing_NR3Generator_keeps_sequence()
        {
            var r1 = new NR3Generator();
            for (var i = 0; i < 10; i++)
            {
                int toss = r1.Next();
            }

            byte[] ser_r1 = ObjectToByteArray(r1);
            string b64_r1 = Convert.ToBase64String(ser_r1);


            var serializer = new Serializer();
            //var writer = new StringWriter();
            //writer.Write(b64_r1);

            var save = new game_save {Generator = b64_r1};
            var yaml_r1 = serializer.Serialize(save);


            var reader = new StringReader(yaml_r1);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new PascalCaseNamingConvention())
                .Build();

            var save_2 = deserializer.Deserialize<game_save>(yaml_r1);


            byte[] ser_r2 = Convert.FromBase64String(save_2.Generator);
            var r2 = (NR3Generator)ByteArrayToObject(ser_r2);

            for (var j = 0; j < 100; j++)
            {
                int r1_out = r1.Next();
                int r2_out = r2.Next();

                Assert.That(r1_out, Is.EqualTo(r2_out));
            }
        }

        [Serializable]
        public class game_save
        {
            public string Generator { get; set; }
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

        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream(arrBytes))
            {
                var binForm = new BinaryFormatter();
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }
    }
}
