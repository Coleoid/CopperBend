using Troschuetz.Random.Generators;
using YamlDotNet.Serialization;
using CopperBend.Contract;
using CopperBend.Fabric;
using NUnit.Framework;
using CopperBend.Model;
using NSubstitute;
using CopperBend.Logic;

namespace CopperBend.Persist.Tests
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

            var scef = Substitute.For<ISadConEntityFactory>();
            Engine.Cosmogenesis("Who wrote the book", scef);
        }

        [Test]
        public void CRT_Compendium()
        {
            var compendium = new Compendium(null)
            {
                TomeOfChaos = new TomeOfChaos("gloop"),
                Herbal = new Herbal(),
                SocialRegister = new SocialRegister(),
                Dramaticon = new Dramaticon(),
            };

            var yaml = _serializer.Serialize(compendium);

            Assert.That(yaml, Is.Not.Null);

            var newBook = _deserializer.Deserialize<IBook>(yaml);

            Assert.That(newBook, Is.TypeOf<Compendium>());
            var newCompendium = (Compendium)newBook;
            Assert.That(newCompendium.TomeOfChaos.TopSeed, Is.EqualTo("gloop"));

            //TODO: slightly more muscular checks once these books go beyond placeholders
            Assert.That(newCompendium.Herbal, Is.Not.Null);
            Assert.That(newCompendium.SocialRegister, Is.Not.Null);
            //Assert.That(newCompendium.Dramaticon, Is.Not.Null);
        }

        [Test]
        public void CRT_TomeOfChaos()
        {
            var tome = new TomeOfChaos("floop");
            var yaml = _serializer.Serialize(tome);

            Assert.That(yaml, Is.Not.Null);

            var newBook = _deserializer.Deserialize<IBook>(yaml);
            Assert.That(newBook, Is.TypeOf<TomeOfChaos>());
            var newTome = (TomeOfChaos)newBook;
            Assert.That(newTome.TopSeed, Is.EqualTo("floop"));
            Assert.That(newTome.TopGenerator, Is.TypeOf<NR3Generator>());
            Assert.That(newTome.LearnableGenerator, Is.TypeOf<NR3Generator>());
            Assert.That(newTome.MapTopGenerator, Is.TypeOf<NR3Generator>());

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
            var thorny = new PlantDetails
            {
                ID = 77,
                MainName = "Thornfriend",
                FruitAdjective = "Luminous",
                FruitKnown = true,
                SeedAdjective = "Knobbly",
                SeedKnown = true,
                GrowthTime = 234,
            };

            var boomy = new PlantDetails
            {
                ID = 88,
                MainName = "Boomer",
                FruitAdjective = "Singed",
                FruitKnown = false,
                SeedAdjective = "Dark",
                SeedKnown = false,
                GrowthTime = 432,
            };

            herbal.AddPlant(thorny);
            herbal.AddPlant(boomy);

            var yaml = _serializer.Serialize(herbal);

            Assert.That(yaml, Is.Not.Null);

            var newBook = _deserializer.Deserialize<IBook>(yaml);
            Assert.That(newBook, Is.TypeOf<Herbal>());
            var newHerbal = (Herbal)newBook;

            var newDetail = newHerbal.PlantByID[77];
            Assert.That(newDetail.MainName, Is.EqualTo("Thornfriend"));
        }

        [Test]
        public void CRT_SocialRegister()
        {
            //if (!Debugger.IsAttached) Debugger.Launch();
            var creator = Engine.BeingCreator;
            var ourHero = creator.CreateBeing("Suvail");
            var reg = new SocialRegister();
            reg.LoadRegister(ourHero);

            var yaml = _serializer.Serialize(reg);

            Assert.That(yaml, Is.Not.Null);

            var newBook = _deserializer.Deserialize<IBook>(yaml);
            Assert.That(newBook, Is.TypeOf<SocialRegister>());
            var newRegister = (SocialRegister)newBook;

            var kellet = newRegister.WellKnownBeings["Kellet Benison"];
            Assert.That(kellet.Name, Is.EqualTo("Kellet Benison"));
            Assert.That(kellet.BeingType, Is.EqualTo("Townsfolk"));
            Assert.That(kellet.Glyph, Is.EqualTo('K'));
        }
    }
}
