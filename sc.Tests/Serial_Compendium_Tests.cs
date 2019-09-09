using System.Diagnostics;
using CopperBend.Contract;
using Newtonsoft.Json;
using NUnit.Framework;
using CopperBend.Persist;
using CopperBend.Fabric;
using Troschuetz.Random.Generators;
using YamlDotNet.Serialization;

namespace sc_tests
{
    [TestFixture]
    public class Serial_Compendium_Tests
    {
        [SetUp]
        public void SetUp()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ContractResolver = DomainContractResolver.Instance,
            };
        }


        [Test]
        public void CRT_Tome_of_Chaos()
        {
            var tome = new TomeOfChaos("floop");
            var json = JsonConvert.SerializeObject(tome);
            var newBook = JsonConvert.DeserializeObject<IBook>(json);
            Assert.That(newBook, Is.TypeOf<TomeOfChaos>());
            var newTome = (TomeOfChaos)newBook;
            Assert.That(newTome.TopSeed, Is.EqualTo("floop"));
        }

        [Test]
        public void Y_CRT_Tome_of_Chaos()
        {
            //if (!Debugger.IsAttached) Debugger.Launch();

            var serializer = new SerializerBuilder()
                .WithTypeConverter(new YConv_IBook())
                .Build();

            var deserializer = new DeserializerBuilder()
                .WithTypeConverter(new YConv_IBook())
                .Build();

            var tome = new TomeOfChaos("floop");
            var yaml = serializer.Serialize(tome);

            Assert.That(yaml, Is.Not.Null);

            var newBook = deserializer.Deserialize<IBook>(yaml);
            Assert.That(newBook, Is.TypeOf<TomeOfChaos>());
            var newTome = (TomeOfChaos)newBook;
            Assert.That(newTome.TopSeed, Is.EqualTo("floop"));
            Assert.That(newTome.TopGenerator, Is.TypeOf<XorShift128Generator>());
            Assert.That(newTome.LearnableGenerator, Is.TypeOf<XorShift128Generator>());
            Assert.That(newTome.MapTopGenerator, Is.TypeOf<XorShift128Generator>());

            for (int i = 0; i < 10; i++)
            {
                Assert.That(newTome.LearnableRndNext(), Is.EqualTo(tome.LearnableRndNext()));
            }
        }

        [Test]
        public void Y_CRT_Compendium()
        {
            var serializer = new SerializerBuilder()
                .WithTypeConverter(new YConv_IBook())
                .Build();

            var deserializer = new DeserializerBuilder()
                .WithTypeConverter(new YConv_IBook())
                .Build();

            var compendium = new Compendium
            {
                TomeOfChaos = new TomeOfChaos("gloop"),
                Herbal = new Herbal(),
                SocialRegister = new SocialRegister(),
                Dramaticon = new Dramaticon(),
            };

            //if (!Debugger.IsAttached) Debugger.Launch();
            var yaml = serializer.Serialize(compendium);

            Assert.That(yaml, Is.Not.Null);

            var newBook = deserializer.Deserialize<IBook>(yaml);
            Assert.That(newBook, Is.TypeOf<Compendium>());
            var newCompendium = (Compendium)newBook;
            Assert.That(newCompendium.BookType, Is.EqualTo("Compendium"));
            Assert.That(newCompendium.TomeOfChaos.TopSeed, Is.EqualTo("gloop"));
            Assert.That(newCompendium.Herbal, Is.Not.Null);
            Assert.That(newCompendium.SocialRegister, Is.Not.Null);
            Assert.That(newCompendium.Dramaticon, Is.Not.Null);
        }
    }

}
