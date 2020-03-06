using Microsoft.Xna.Framework;
using SadConsole.Entities;
using YamlDotNet.Serialization;
using CopperBend.Contract;
using CopperBend.Model;
using NSubstitute;
using NUnit.Framework;
using CopperBend.Fabric;
using CopperBend.Logic;

namespace CopperBend.Persist.Tests
{
    [TestFixture]
    public class Serial_Being_Tests
    {
        private ISerializer _serializer;
        private IDeserializer _deserializer;
        private BeingCreator _beingCreator;

        [SetUp]
        public void SetUp()
        {
            var ycIBeing = new YConv_IBeing();

            _serializer = new SerializerBuilder()
                .WithTypeConverter(ycIBeing)
                .Build();

            _deserializer = new DeserializerBuilder()
                .WithTypeConverter(ycIBeing)
                .Build();

            var entityFactory = Substitute.For<ISadConEntityFactory>();
            entityFactory.GetSadCon(Arg.Any<ISadConInitData>())
                .Returns(Substitute.For<IEntity>());
            Engine.Cosmogenesis("serial being!", entityFactory);
            _beingCreator = Engine.BeingCreator;
            ycIBeing.BeingCreator = _beingCreator;
        }

        [Test]
        public void CRT_Being()
        {
            //if (!Debugger.IsAttached) Debugger.Launch();
            var being = _beingCreator.CreateBeing(Color.Bisque, Color.Azure, 'g', 77);

            var yaml = _serializer.Serialize(being);
            Assert.That(yaml, Is.Not.Null);
            var newBeing = _deserializer.Deserialize<IBeing>(yaml);
            Assert.That(newBeing, Is.TypeOf<Being>());

            Assert.That(newBeing.Foreground, Is.EqualTo(being.Foreground));
            Assert.That(newBeing.Background, Is.EqualTo(being.Background));
            Assert.That((char)newBeing.Glyph, Is.EqualTo((char)being.Glyph));
            Assert.That(newBeing.ID, Is.EqualTo(being.ID));
        }

        [Test]
        public void CRT_Being_full()
        {
            //if (!Debugger.IsAttached) Debugger.Launch();
            var being = _beingCreator.CreateBeing(Color.DarkSeaGreen, Color.Black, 'g', 77);
            being.Name = "Gred";
            being.Awareness = 2;
            being.Health = 22;
            being.Position = new Point(4, 5);

            var yaml = _serializer.Serialize(being);
            Assert.That(yaml, Is.Not.Null);
            var newBeing = _deserializer.Deserialize<IBeing>(yaml);
            Assert.That(newBeing, Is.TypeOf<Being>());

            Assert.That(newBeing.Name, Is.EqualTo(being.Name));
            Assert.That(newBeing.Awareness, Is.EqualTo(being.Awareness));
            Assert.That(newBeing.Health, Is.EqualTo(being.Health));
            Assert.That(newBeing.Position, Is.EqualTo(being.Position));
        }

        //[Test]
        //public void CRT_Monster()
        //{
        //    var being = new Monster(Color.Bisque, Color.Azure, 'g', 33);

        //    var yaml = _serializer.Serialize(being);
        //    Assert.That(yaml, Is.Not.Null);
        //    var newBeing = _deserializer.Deserialize<IBeing>(yaml);
        //    Assert.That(newBeing, Is.TypeOf<Monster>());

        //    Assert.That(newBeing.Name, Is.EqualTo(being.Name));
        //    Assert.That(newBeing.Foreground, Is.EqualTo(being.Foreground));
        //    Assert.That(newBeing.Background, Is.EqualTo(being.Background));
        //    Assert.That(newBeing.Glyph, Is.EqualTo(being.Glyph));
        //    Assert.That(newBeing.ID, Is.EqualTo(being.ID));
        //}

        //[Test]
        //public void CRT_Player()
        //{
        //    var being = new Player(Color.Green, Color.Black, '@', 44);
        //    var yaml = _serializer.Serialize(being);
        //    Assert.That(yaml, Is.Not.Null);
        //    var newBeing = _deserializer.Deserialize<IBeing>(yaml);
        //    Assert.That(newBeing, Is.TypeOf<Player>());

        //    var newPlayer = (Player)newBeing;
        //    Assert.That(newPlayer.Name, Is.EqualTo(being.Name));
        //    Assert.That(newPlayer.Foreground, Is.EqualTo(being.Foreground));
        //    Assert.That(newPlayer.Background, Is.EqualTo(being.Background));
        //    Assert.That(newPlayer.Glyph, Is.EqualTo(being.Glyph));
        //    Assert.That(newPlayer.ID, Is.EqualTo(being.ID));
        //}

        //[Test]
        //public void CRT_Player_inventory()
        //{
        //    var being = new Player(Color.Green, Color.Black, '@', 44);
        //    being.AddToInventory(new Knife((3,3), id: 17));
        //    being.AddToInventory(new Hoe((1,1), id: 23));

        //    var yaml = _serializer.Serialize(being);
        //    Assert.That(yaml, Is.Not.Null);
        //    var newBeing = _deserializer.Deserialize<IBeing>(yaml);
        //    Assert.That(newBeing, Is.TypeOf<Player>());

        //    var newPlayer = (Player)newBeing;
        //    var inv = newPlayer.Inventory.ToList();
        //    Assert.That(inv.Count, Is.EqualTo(2));
        //    Assert.That(inv[0].ID, Is.EqualTo(17));
        //    Assert.That(inv[0].Location, Is.EqualTo((3,3)));
        //    Assert.That(inv[0], Is.TypeOf<Knife>());
        //    Assert.That(inv[1].ID, Is.EqualTo(23));
        //    Assert.That(inv[1].ItemType, Is.EqualTo("Hoe"));
        //}
    }
}
