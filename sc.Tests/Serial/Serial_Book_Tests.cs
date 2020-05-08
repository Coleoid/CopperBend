using Troschuetz.Random.Generators;
using YamlDotNet.Serialization;
using CopperBend.Contract;
using CopperBend.Fabric;
using NUnit.Framework;
using CopperBend.Model;
using GoRogue;
using CopperBend.Fabric.Tests;
using Microsoft.Xna.Framework;

namespace CopperBend.Persist.Tests
{
    [TestFixture]
    public class Serial_Book_Tests
    {
        private ISerializer _serializer;
        private IDeserializer _deserializer;
        private ISadConEntityFactory __factory;
        private BeingCreator _creator;

        [SetUp]
        public void SetUp()
        {
            __factory = UTHelp.GetSubstituteFactory();

            _creator = new BeingCreator(__factory);

            _serializer = new SerializerBuilder()
                .WithTypeConverter(new YConv_IBook(_creator))
                .Build();

            _deserializer = new DeserializerBuilder()
                .WithTypeConverter(new YConv_IBook(_creator))
                .Build();
        }

        [Test]
        public void CRT_Compendium()
        {
            var creator = new BeingCreator(__factory);
            var publisher = new BookPublisher(_creator);
            var tomeOfChaos = publisher.Tome_FromNew("gloop");
            var herbal = new Herbal();
            var socialRegister = new SocialRegister(creator);
            var dramaticon = new Dramaticon();
            var atlas = new Atlas();

            var idGen = new IDGenerator(33);
            var compendium = new Compendium(idGen, creator, tomeOfChaos, herbal, socialRegister, dramaticon, atlas);

            var yaml = _serializer.Serialize(compendium);
            Assert.That(yaml, Is.Not.Null);

            var newBook = _deserializer.Deserialize<IBook>(yaml);

            Assert.That(newBook, Is.TypeOf<Compendium>());
            var newCompendium = (Compendium)newBook;

            Assert.That(newCompendium.IDGenerator.UseID(), Is.EqualTo(33));
            Assert.That(newCompendium.TomeOfChaos.TopSeed, Is.EqualTo("gloop"));

            //TODO: slightly more muscular checks once these books go beyond placeholders
            Assert.That(newCompendium.Herbal, Is.Not.Null);
            Assert.That(newCompendium.SocialRegister, Is.Not.Null);
            Assert.That(newCompendium.Dramaticon, Is.Not.Null);
            Assert.That(newCompendium.Atlas, Is.Not.Null);
        }

        [Test]
        public void CRT_TomeOfChaos()
        {
            var publisher = new BookPublisher(_creator);
            var tome = publisher.Tome_FromNew("floop");
            var yaml = _serializer.Serialize(tome);

            Assert.That(yaml, Is.Not.Null);

            var newBook = _deserializer.Deserialize<IBook>(yaml);
            Assert.That(newBook, Is.TypeOf<TomeOfChaos>());
            var newTome = (TomeOfChaos)newBook;
            Assert.That(newTome.TopSeed, Is.EqualTo("floop"));
            Assert.That(newTome.Generators["Top"], Is.TypeOf<NR3Generator>());
            Assert.That(newTome.Generators["Learnable"], Is.TypeOf<NR3Generator>());
            Assert.That(newTome.Generators["MapTop"], Is.TypeOf<NR3Generator>());

            for (int i = 0; i < 10; i++)
            {
                Assert.That(newTome.LearnableRndNext(), Is.EqualTo(tome.LearnableRndNext()),
                    "The numbers emitted from a serialized & deserialized RNG should match the RNG it was originally serialized from.");
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
            var creator = new BeingCreator(__factory);
            var ourHero = creator.CreateBeing("Suvail");
            var reg = new SocialRegister(creator);
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

        [Test]
        public void CRT_Dramaticon()
        {
            //if (!Debugger.IsAttached) Debugger.Launch();
            var drama = new Dramaticon
            {
                HasClearedRot = true
            };

            var yaml = _serializer.Serialize(drama);
            Assert.That(yaml, Is.Not.Null);

            var newBook = _deserializer.Deserialize<IBook>(yaml);
            Assert.That(newBook, Is.TypeOf<Dramaticon>());
            var newDramaticon = (Dramaticon)newBook;

            Assert.That(newDramaticon.HasClearedRot);
        }

        [Test]
        public void CRT_Atlas()
        {
            //if (!Debugger.IsAttached) Debugger.Launch();
            var atlas = new Atlas();
            atlas.StoreTerrain(new Terrain
            {
                Name = "CanRoundTrip",
                Cell = new SadConsole.Cell(new Color(1, 2, 3, 4), new Color(5, 6, 7, 8), 'C'),
                CanPlant = true,
                CanSeeThrough = true,
                CanWalkThrough = true,
            });

            var yaml = _serializer.Serialize(atlas);
            Assert.That(yaml, Is.Not.Null);

            var newBook = _deserializer.Deserialize<IBook>(yaml);
            Assert.That(newBook, Is.TypeOf<Atlas>());
            var newAtlas = (Atlas)newBook;

            Assert.That(newAtlas.Legend, Is.Not.Null);
            var terrain = newAtlas.Legend["CanRoundTrip"];
            Assert.That(terrain, Is.Not.Null);
            Assert.That(terrain.CanPlant);
            Assert.That(terrain.CanSeeThrough);
            Assert.That(terrain.CanWalkThrough);
            Assert.That(terrain.Cell.Foreground.R, Is.EqualTo(1));
            Assert.That(terrain.Cell.Foreground.A, Is.EqualTo(4));
        }
    }
}
