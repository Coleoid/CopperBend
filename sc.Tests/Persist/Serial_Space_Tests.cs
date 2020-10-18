//using YamlDotNet.Serialization;
//using CopperBend.Contract;
//using CopperBend.Fabric;
//using NUnit.Framework;

////  CanRT_* names:  Can Round-Trip an object to YAML and back.
//namespace CopperBend.Persist.Tests
//{
//    [TestFixture]
//    public class Serial_Space_Tests
//    {
//        private ISerializer _serializer;
//        private IDeserializer _deserializer;

//        [SetUp]
//        public void SetUp()
//        {
//            _serializer = new SerializerBuilder()
//                .EnsureRoundtrip()
//                .WithTypeConverter(new YConv_ISpace())
//                .Build();

//            _deserializer = new DeserializerBuilder()
//                .WithTypeConverter(new YConv_ISpace())
//                .Build();
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
//                Name = "berber carpet",
//                CanPlant = true,
//                CanSeeThrough = true,
//                CanWalkThrough = true,
//            };

//            var yaml = _serializer.Serialize(terrain);
//            Assert.That(yaml, Is.Not.Null);
//            var newTerrain = _deserializer.Deserialize<Terrain>(yaml);
//            Assert.That(newTerrain, Is.TypeOf<Terrain>());

//            //Assert.That(newTerrain.ID, Is.EqualTo(terrain.ID));
//            //Assert.That(newTerrain.IsKnown, Is.EqualTo(terrain.IsKnown));
//            //Assert.That(newTerrain.IsSown, Is.EqualTo(terrain.IsSown));
//            //Assert.That(newTerrain.IsTilled, Is.EqualTo(terrain.IsTilled));
//            Assert.That(newTerrain.Name, Is.EqualTo(terrain.Name));
//        }
//    }
//}
