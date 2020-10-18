using System.Collections.Generic;
using YamlDotNet.Serialization;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;
using NUnit.Framework;
using CopperBend.Logic.Tests;
using Microsoft.Xna.Framework;
using System.Linq;

namespace CopperBend.Creation.Tests
{
    [TestFixture]
    public class MapLoader_Being_Tests : Tests_Base
    {
        protected override bool ShouldPrepDI => true;
        protected override MockableServices GetServicesToMock()
        {
            return MockableServices.Log
                | MockableServices.EntityFactory
                | base.GetServicesToMock();
        }

        private ISerializer _serializer;
        private IDeserializer _deserializer;
        private IBeingCreator _beingCreator;

        [SetUp]
        public void SetUp()
        {
            Basis.ConnectIDGenerator();
            _beingCreator = SourceMe.The<IBeingCreator>();

            var _loader = SourceMe.The<MapLoader>();
            _serializer = _loader.GetSerializer();
            _deserializer = _loader.GetDeserializer();
        }

        [TestCase(ColorEn.DarkSeaGreen, ColorEn.Black, 'g', 77u, "Gred", 2, 22, "Dude")]
        [TestCase(ColorEn.BurlyWood, ColorEn.BlueViolet, 'G', 383u, "I am Groot", 3, 140, "Groot")]
        public void CanRT_Being_full(ColorEn fgEn, ColorEn bgEn, char glyph, uint id, string name, int awareness, int health, string type)
        {
            //DBG();
            var fg = ColorOf(fgEn);
            var bg = ColorOf(bgEn);
            var being = _beingCreator.CreateBeing(fg, bg, glyph, id: id);
            being.Name = name;
            being.Awareness = awareness;
            being.Health = health;
            being.BeingType = type;

            var yaml = _serializer.Serialize(being);
            Assert.That(yaml, Is.Not.Null);
            var newBeing = _deserializer.Deserialize<IBeing>(yaml);
            Assert.That(newBeing, Is.TypeOf<Model.Being>());

            Assert.That(newBeing.Foreground, Is.EqualTo(fg));
            Assert.That(newBeing.Background, Is.EqualTo(bg));
            Assert.That(newBeing.Glyph, Is.EqualTo(glyph));
            Assert.That(newBeing.ID, Is.EqualTo(id));
            Assert.That(newBeing.Name, Is.EqualTo(name));
            Assert.That(newBeing.Awareness, Is.EqualTo(awareness));
            Assert.That(newBeing.Health, Is.EqualTo(health));
            Assert.That(newBeing.BeingType, Is.EqualTo(type));
        }

        [Test]
        public void CanRT_Coord_IBeing()
        {
            var c = new Coord(4, 9);
            var b = _beingCreator.CreateBeing("Suvail");
            var cib = new Coord_IBeing { Coord = c, Being = b };

            var yaml = _serializer.Serialize(cib);
            Assert.That(yaml, Is.Not.Null);
            var newST = _deserializer.Deserialize<Coord_IBeing>(yaml);

            Assert.That(newST.Coord.X, Is.EqualTo(4));
            Assert.That(newST.Coord.Y, Is.EqualTo(9));
        }

        [Test]
        public void CanRT_MultiBeings_Collection()
        {
            var c = new Coord(4, 9);
            var b = _beingCreator.CreateBeing("Suvail");
            var cib = new Coord_IBeing { Coord = c, Being = b };

            var mb = new Dictionary<uint, Coord_IBeing>();
            mb[b.ID] = cib;

            var yaml = _serializer.Serialize(mb);
            Assert.That(yaml, Is.Not.Null);

            var newMB = _deserializer.Deserialize<Dictionary<uint, Coord_IBeing>>(yaml);

            var newST = newMB[b.ID];
            Assert.That(newST.Coord.X, Is.EqualTo(4));
            Assert.That(newST.Coord.Y, Is.EqualTo(9));

            var newBeing = newST.Being;
            Assert.That(newBeing.Name, Is.EqualTo("Suvail"));
        }

        [Test]
        public void CRT_Being_inventory()
        {
            var being = _beingCreator.CreateBeing(Color.Green, Color.Black, '@');
            var item = new Item(120, 17)
            {
                Name = "Fluffy",
                ItemType = "Stuffed Toy",
                Glyph = '*',
                Adjective = "So",
                Foreground = Color.BlanchedAlmond,
            };
            var lump = new Item(1, 23)
            {
                Name = "lump",
                ItemType = "Thing",
                Glyph = '.',
                Adjective = "Nondescript",
                Foreground = Color.DarkKhaki,
            };

            being.AddToInventory(item);
            being.AddToInventory(lump);

            var yaml = _serializer.Serialize(being);
            Assert.That(yaml, Is.Not.Null);
            var newPlayer = _deserializer.Deserialize<IBeing>(yaml);

            var inv = newPlayer.Inventory.ToList();
            Assert.That(inv.Count, Is.EqualTo(2));
            Assert.That(inv[0].ID, Is.EqualTo(17));
            Assert.That(inv[1].ID, Is.EqualTo(23));
            Assert.That(inv[1].ItemType, Is.EqualTo("Thing"));
        }

        [Test]
        public void CRT_Being_Strategy()
        {
            var being = _beingCreator.CreateBeing(Color.Khaki, Color.Black, 'M');
            being.StrategyStyle = StrategyStyle.UserInput;
            being.StrategyStorage["foo"] = "bar";
            being.StrategyStorage["baz"] = "qux";

            var yaml = _serializer.Serialize(being);
            Assert.That(yaml, Is.Not.Null);
            var newBeing = _deserializer.Deserialize<IBeing>(yaml);

            var stratCat = newBeing.StrategyStyle;
            Assert.That(stratCat, Is.EqualTo(StrategyStyle.UserInput));

            Assert.That(newBeing.StrategyStorage["foo"], Is.EqualTo("bar"));
            Assert.That(newBeing.StrategyStorage["baz"], Is.EqualTo("qux"));
        }
    }
}
