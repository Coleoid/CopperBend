using NUnit.Framework;
using YamlDotNet.Serialization;
using CopperBend.Fabric;
using CopperBend.Model;
using CopperBend.Logic;
using CopperBend.Fabric.Tests;
using Microsoft.Xna.Framework;
using CopperBend.Contract;
using GoRogue;

namespace CopperBend.Persist.Tests
{
    [TestFixture]
    public class Serial_TagMapping_Map_Tests
    {
        private ISerializer _serzr;
        private IDeserializer _deszr;
        private BeingCreator _beingCreator;

        [SetUp]
        public void SetUp()
        {
            var entityFactory = UTHelp.GetSubstituteFactory();
            Engine.Cosmogenesis("serial being!", entityFactory);
            _beingCreator = Engine.BeingCreator;

            var ycIBeing = new YConv_IBeing { BeingCreator = _beingCreator };

            _serzr = new SerializerBuilder()
                .EnsureRoundtrip()
                .WithTagMapping("!RotMap", typeof(RotMap))
                .WithTagMapping("!AreaRot", typeof(AreaRot))
                .WithTagMapping("!CompoundMapData", typeof(CompoundMapData))
                .WithTagMapping("!Item", typeof(Item))
                .WithTagMapping("!AttackMethod", typeof(AttackMethod))
                .WithTypeConverter(ycIBeing)
                .WithTypeConverter(new YConv_Coord())
//                .WithTagMapping("!Coord", typeof(Coord))
                .Build();

            _deszr = new DeserializerBuilder()
                .WithTagMapping("!RotMap", typeof(RotMap))
                .WithTagMapping("!AreaRot", typeof(AreaRot))
                .WithTagMapping("!CompoundMapData", typeof(CompoundMapData))
                .WithTagMapping("!Item", typeof(Item))
                .WithTagMapping("!AttackMethod", typeof(AttackMethod))
                .WithTypeConverter(ycIBeing)
                .WithTypeConverter(new YConv_Coord())
                //.WithTagMapping("!Coord", typeof(Coord))
                .Build();

            AreaRot.SetIDGenerator(new GoRogue.IDGenerator());
        }

        [Test]
        public void CompoundMapData_RT()
        {
            var md = new CompoundMapData
            {
                Name = "MapData RoundTrip",
                Width = 22,
                Height = 40,
            };

            md.Legend["."] = "dirt";
            md.Legend["#"] = "wall";
            md.Legend["+"] = "door closed";
            md.Legend["-"] = "door open";

            md.Terrain.Add("#####");
            md.Terrain.Add("#...#");
            md.Terrain.Add("#+#.#");
            md.Terrain.Add("#.-.#");
            md.Terrain.Add("#####");

            md.AreaRots[(2,3)] = new AreaRot(98);

            var vel = _beingCreator.CreateBeing(Color.Aqua, Color.Black, 'v', id: 313);
            md.Beings[(3,4)] = vel;

            md.Items[(4,5)] = Equipper.BuildItem("knife", 2);
            md.Items[(5,6)] = Equipper.BuildItem("widget", 1);
            md.Items[(6,7)] = Equipper.BuildItem("gadget", 3);

            md.FirstSightMessage.Add("I know kung fu.");


            var yaml = _serzr.Serialize(md);
            Assert.That(yaml, Is.Not.Null);
            var newMD = _deszr.Deserialize<CompoundMapData>(yaml);

            Assert.That(newMD.Name, Is.EqualTo(md.Name));
            Assert.That(newMD.Width, Is.EqualTo(md.Width));
            Assert.That(newMD.Height, Is.EqualTo(md.Height));

            Assert.That(newMD.Legend.Count, Is.EqualTo(md.Legend.Count));
            Assert.That(newMD.Legend["."], Is.EqualTo(md.Legend["."]));

            Assert.That(newMD.Terrain.Count, Is.EqualTo(md.Terrain.Count));
            Assert.That(newMD.Terrain[0], Is.EqualTo(md.Terrain[0]));

            Assert.That(newMD.AreaRots.Count, Is.EqualTo(md.AreaRots.Count));
            Assert.That(newMD.AreaRots[(2,3)].Health, Is.EqualTo(md.AreaRots[(2,3)].Health));

            Assert.That(newMD.Beings.Count, Is.EqualTo(md.Beings.Count));
            var newVel = newMD.Beings[(3,4)];
            Assert.That(newVel.ID, Is.EqualTo(vel.ID));
            Assert.That(newVel.Foreground, Is.EqualTo(Color.Aqua));

            Assert.That(newMD.Items.Count, Is.EqualTo(md.Items.Count));
            Assert.That(newMD.Items[(4,5)].ItemType, Is.EqualTo(md.Items[(4,5)].ItemType));
            Assert.That(newMD.Items[(4,5)].Quantity, Is.EqualTo(md.Items[(4,5)].Quantity));

            Assert.That(newMD.Items.Count, Is.EqualTo(md.Items.Count));
            var newGadget = newMD.Items[(6,7)];
            Assert.That(newGadget.Quantity, Is.EqualTo(3));

            Assert.That(newMD.FirstSightMessage[0], Is.EqualTo(md.FirstSightMessage[0]));
        }

        [Test]
        public void Coord_RT()
        {
            var coord = new Coord(4, 9);

            var yaml = _serzr.Serialize(coord);
            Assert.That(yaml, Is.Not.Null);
            var newCoord = _deszr.Deserialize<Coord>(yaml);

            Assert.That(newCoord.X, Is.EqualTo(4));
            Assert.That(newCoord.Y, Is.EqualTo(9));
        }


        [Test]
        public void AreaRot_RT()
        {
            var rot = new AreaRot(22) { Health = 14 };

            var yaml = _serzr.Serialize(rot);
            Assert.That(yaml, Is.Not.Null);
            var newRot = _deszr.Deserialize<AreaRot>(yaml);

            Assert.That(newRot.ID, Is.EqualTo(22));
            Assert.That(newRot.Health, Is.EqualTo(14));
        }


        [Test]
        public void Being_RT()
        {
            var being = _beingCreator.CreateBeing(Color.BurlyWood, Color.BlueViolet, 'b', id: 383);
            being.Awareness = 3;
            being.BeingType = "Groot";
            being.Name = "I am Groot";

            var yaml = _serzr.Serialize(being);
            Assert.That(yaml, Is.Not.Null);
            var newBeing = _deszr.Deserialize<IBeing>(yaml);

            Assert.That(newBeing.ID, Is.EqualTo(383));
            Assert.That(newBeing.Health, Is.EqualTo(20));
        }



        [Test]
        public void Item_RT()
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
