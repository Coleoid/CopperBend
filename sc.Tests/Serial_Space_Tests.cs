using YamlDotNet.Serialization;
using NUnit.Framework;
using CopperBend.Contract;
using CopperBend.Fabric;

namespace CopperBend.Persist.Tests
{
    [TestFixture]
    public class Serial_Space_Tests
    {
        private ISerializer _serializer;
        private IDeserializer _deserializer;

        [SetUp]
        public void SetUp()
        {
            _serializer = new SerializerBuilder()
                .EnsureRoundtrip()
                .WithTypeConverter(new YConv_ISpace())
                .Build();

            _deserializer = new DeserializerBuilder()
                .WithTypeConverter(new YConv_ISpace())
                .Build();
        }

        [Test]
        public void CRT_Space()
        {
            var terrain = new TerrainType
            {
                Name = "berber carpet",
                CanPlant = true,
                CanSeeThrough = true,
                CanWalkThrough = true,
            };
            //if (!Debugger.IsAttached) Debugger.Launch();
            var space = new Space(22)
            {
                Terrain = terrain,
                IsKnown = true,
                IsSown = true,
                IsTilled = true,
            };

            var yaml = _serializer.Serialize(space);
            Assert.That(yaml, Is.Not.Null);
            var newSpace = _deserializer.Deserialize<ISpace>(yaml);
            Assert.That(newSpace, Is.TypeOf<Space>());

            Assert.That(newSpace.ID, Is.EqualTo(space.ID));
            Assert.That(newSpace.IsKnown, Is.EqualTo(space.IsKnown));
            Assert.That(newSpace.IsSown, Is.EqualTo(space.IsSown));
            Assert.That(newSpace.IsTilled, Is.EqualTo(space.IsTilled));
            //Assert.That(newSpace.Terrain.Name, Is.EqualTo(space.Terrain.Name));
        }
    }
}
