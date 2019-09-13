using NUnit.Framework;
using Troschuetz.Random.Generators;
using YamlDotNet.Serialization;
using CopperBend.Contract;
using CopperBend.Persist;
using CopperBend.Fabric;

namespace sc_tests
{
    [TestFixture]
    public class Serial_Compendium_Tests
    {
        private ISerializer _serializer;
        private IDeserializer _deserializer;

        [SetUp]
        public void SetUp()
        {
            _serializer = new SerializerBuilder()
                .WithTypeConverter(new YConv_IBook())
                .Build();

            _deserializer = new DeserializerBuilder()
                .WithTypeConverter(new YConv_IBook())
                .Build();
        }

        [Test]
        public void CRT_Compendium()
        {
            var compendium = new Compendium
            {
                TomeOfChaos = new TomeOfChaos("gloop"),
                Herbal = new Herbal(),
                SocialRegister = new SocialRegister(),
                Dramaticon = new Dramaticon(),
            };

            //if (!Debugger.IsAttached) Debugger.Launch();
            var yaml = _serializer.Serialize(compendium);

            Assert.That(yaml, Is.Not.Null);

            var newBook = _deserializer.Deserialize<IBook>(yaml);

            Assert.That(newBook, Is.TypeOf<Compendium>());
            var newCompendium = (Compendium)newBook;
            Assert.That(newCompendium.BookType, Is.EqualTo("Compendium"));
            Assert.That(newCompendium.TomeOfChaos.TopSeed, Is.EqualTo("gloop"));

            //TODO: slightly more muscular checks once these books go beyond placeholders
            Assert.That(newCompendium.Herbal, Is.Not.Null);
            Assert.That(newCompendium.SocialRegister, Is.Not.Null);
            Assert.That(newCompendium.Dramaticon, Is.Not.Null);
        }

        [Test]
        public void CRT_TomeOfChaos()
        {
            //if (!Debugger.IsAttached) Debugger.Launch();

            var tome = new TomeOfChaos("floop");
            var yaml = _serializer.Serialize(tome);

            Assert.That(yaml, Is.Not.Null);

            var newBook = _deserializer.Deserialize<IBook>(yaml);
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
        public void CRT_Herbal()
        {
            //if (!Debugger.IsAttached) Debugger.Launch();

            var herbal = new Herbal();
            var yaml = _serializer.Serialize(herbal);

            Assert.That(yaml, Is.Not.Null);

            var newBook = _deserializer.Deserialize<IBook>(yaml);
            Assert.That(newBook, Is.TypeOf<Herbal>());
            var newHerbal = (Herbal)newBook;

            Assert.That(newHerbal.PlantByID[1].MainName, Is.EqualTo("Thornfriend"));
            //Assert.That(newHerbal.TopGenerator, Is.TypeOf<XorShift128Generator>());
            //Assert.That(newHerbal.LearnableGenerator, Is.TypeOf<XorShift128Generator>());
            //Assert.That(newHerbal.MapTopGenerator, Is.TypeOf<XorShift128Generator>());
        }
    }

}
