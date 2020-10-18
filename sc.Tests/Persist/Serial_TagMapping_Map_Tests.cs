using NUnit.Framework;
using YamlDotNet.Serialization;
using CopperBend.Model;
using CopperBend.Logic;
using Microsoft.Xna.Framework;
using CopperBend.Contract;
using GoRogue;
using NSubstitute;
using SadConsole.Entities;
using System.Collections.ObjectModel;
using SadConsole.Components;
using CopperBend.Logic.Tests;

namespace CopperBend.Creation.Tests
{
    [TestFixture]
    public class Serial_TagMapping_Map_Tests : Tests_Base
    {
        protected override bool ShouldPrepDI => true;
        protected override MockableServices GetServicesToMock()
        {
            return MockableServices.Log
                | MockableServices.EntityFactory
                | base.GetServicesToMock();
        }

        private ISerializer _serzr;
        private IDeserializer _deszr;
        private MapLoader _mapLoader;

        [SetUp]
        public void SetUp()
        {
            __factory
                .When(f => f.SetIEntityOnPort(Arg.Any<IEntityInitPort>()))
                .Do(ci => {
                    var pi = ci.Arg<IEntityInitPort>();
                    var ie = Substitute.For<IEntity>();
                    var comps = Substitute.For<ObservableCollection<IConsoleComponent>>();
                    ie.Components.Returns(comps);
                    pi.SadConEntity = ie;
                });

            Engine.Cosmogenesis("tag map!", __factory);

            _mapLoader = SourceMe.The<MapLoader>();
            _serzr = _mapLoader.GetSerializer();
            _deszr = _mapLoader.GetDeserializer();

            AreaRot.SetIDGenerator(new IDGenerator());
        }

        [Test]
        public void CanRT_Coord()
        {
            var coord = new Coord(4, 9);

            var yaml = _serzr.Serialize(coord);
            Assert.That(yaml, Is.Not.Null);
            var newCoord = _deszr.Deserialize<Coord>(yaml);

            Assert.That(newCoord.X, Is.EqualTo(4));
            Assert.That(newCoord.Y, Is.EqualTo(9));
        }

        [Test]
        public void CanRT_AreaRot()
        {
            var rot = new AreaRot(22) { Health = 14 };

            var yaml = _serzr.Serialize(rot);
            Assert.That(yaml, Is.Not.Null);
            var newRot = _deszr.Deserialize<AreaRot>(yaml);

            Assert.That(newRot.ID, Is.EqualTo(22));
            Assert.That(newRot.Health, Is.EqualTo(14));
        }

        [Test]
        public void CanRT_Item()
        {
            var item = new Item(2, 246)
            {
                Name = "knobule",
                Adjective = "tobby",
                Glyph = '*',
                ItemType = ItemEnum.Widget,
                Foreground = Color.Bisque,
            };

            var yaml = _serzr.Serialize(item);
            Assert.That(yaml, Is.Not.Null);
            var newItem = _deszr.Deserialize<Item>(yaml);

            Assert.That(newItem.ID, Is.EqualTo(246));
            Assert.That(newItem.Quantity, Is.EqualTo(item.Quantity));
            Assert.That(newItem.Name, Is.EqualTo(item.Name));
            Assert.That(newItem.Adjective, Is.EqualTo(item.Adjective));
            Assert.That(newItem.Glyph, Is.EqualTo(item.Glyph));
            Assert.That(newItem.ItemType, Is.EqualTo(item.ItemType));
            Assert.That(newItem.Foreground, Is.EqualTo(item.Foreground));
        }
    }
}
