//using NUnit.Framework;
//using YamlDotNet.Serialization;
//using CopperBend.Contract;
//using CopperBend.Fabric;
//using CopperBend.Model;
//using CopperBend.Logic;
//using CopperBend.Fabric.Tests;

//namespace CopperBend.Persist.Tests
//{
//    [TestFixture]
//    public class Serial_Map_Tests
//    {
//        private ISerializer _serializer;
//        private IDeserializer _deserializer;
//        private BeingCreator _beingCreator;

//        [SetUp]
//        public void SetUp()
//        {
//            var entityFactory = UTHelp.GetSubstituteFactory();
//            Engine.Cosmogenesis("serial being!", entityFactory);
//            _beingCreator = Engine.BeingCreator;

//            var ycIBeing = new YConv_IBeing { BeingCreator = _beingCreator };

//            _serializer = new SerializerBuilder()
//                .EnsureRoundtrip()
//                .WithTypeConverter(new YConv_ISpace())
//                .WithTypeConverter(ycIBeing)
//                .Build();

//            _deserializer = new DeserializerBuilder()
//                .WithTypeConverter(new YConv_ISpace())
//                .WithTypeConverter(ycIBeing)
//                .Build();
//        }

//        [Test]
//        public void CRT_SpaceMap()
//        {
//            var map = new SpaceMap(5, 5)
//            {
//                PlayerStartPoint = (3, 3)
//            };

//            map.Add(new Space(888), (4, 4));

//            var yaml = _serializer.Serialize(map);
//            Assert.That(yaml, Is.Not.Null);
//            var newMap = _deserializer.Deserialize<SpaceMap>(yaml);

//            Assert.That(newMap.Height, Is.EqualTo(5));
//            Assert.That(newMap.Width, Is.EqualTo(5));
//            Assert.That(newMap.PlayerStartPoint, Is.EqualTo((3, 3)));

//            var space = newMap.GetItem((4, 4));
//            Assert.That(space, Is.Not.Null);
//        }

//        [Test]
//        public void CRT_Space()
//        {
//            var terrain = new Terrain
//            {
//                Name = "berber carpet",
//                CanPlant = true,
//                CanSeeThrough = true,
//                CanWalkThrough = true,
//            };
//            //if (!Debugger.IsAttached) Debugger.Launch();
//            var space = new Space(22)
//            {
//                Terrain = terrain,
//                IsKnown = true,
//                IsSown = true,
//                IsTilled = true,
//            };

//            var yaml = _serializer.Serialize(space);
//            Assert.That(yaml, Is.Not.Null);
//            var newSpace = _deserializer.Deserialize<ISpace>(yaml);
//            Assert.That(newSpace, Is.TypeOf<Space>());

//            Assert.That(newSpace.ID, Is.EqualTo(space.ID));
//            Assert.That(newSpace.IsKnown, Is.EqualTo(space.IsKnown));
//            Assert.That(newSpace.IsSown, Is.EqualTo(space.IsSown));
//            Assert.That(newSpace.IsTilled, Is.EqualTo(space.IsTilled));
//            Assert.That(newSpace.Terrain.Name, Is.EqualTo(space.Terrain.Name));
//        }

//        [Test]
//        public void CRT_Terrain()
//        {
//            var terrain = new Terrain
//            {
//                Name = "dirt road",
//                CanPlant = false,
//                CanSeeThrough = true,
//                CanWalkThrough = true,
//            };


//            var yaml = _serializer.Serialize(terrain);
//            Assert.That(yaml, Is.Not.Null);
//            var newTT = _deserializer.Deserialize<Terrain>(yaml);

//            Assert.That(newTT.Name, Is.EqualTo(terrain.Name));
//            Assert.That(newTT.CanPlant, Is.EqualTo(terrain.CanPlant));
//            Assert.That(newTT.CanSeeThrough, Is.EqualTo(terrain.CanSeeThrough));
//            Assert.That(newTT.CanWalkThrough, Is.EqualTo(terrain.CanWalkThrough));
//        }

//        [Test]
//        public void CRT_RotMap()
//        {
//            //0.2: int ctor arg = deserializing workaround
//            var map = new RotMap();
//            var rot = new AreaRot(888) { Health = 11 };
//            map.Add(rot, (7, 11));
//            map.Add(new AreaRot() { Health = 8 }, (7, 12));

//            var yaml = _serializer.Serialize(map);
//            Assert.That(yaml, Is.Not.Null);
//            var newMap = _deserializer.Deserialize<RotMap>(yaml);

//            //Assert.That(newMap.Name, Is.EqualTo("Bofungus"));
//            var entry = newMap.GetItem((7, 11));
//            Assert.That(entry, Is.Not.Null);
//            Assert.That(entry.ID, Is.EqualTo(888));
//        }

//        [Test]
//        public void CRT_AreaRot()
//        {
//            var rot = new AreaRot(22) { Health = 14 };

//            var yaml = _serializer.Serialize(rot);
//            Assert.That(yaml, Is.Not.Null);
//            var newRot = _deserializer.Deserialize<AreaRot>(yaml);

//            Assert.That(newRot.ID, Is.EqualTo(22));
//            Assert.That(newRot.Health, Is.EqualTo(14));
//        }

//    }
//}
