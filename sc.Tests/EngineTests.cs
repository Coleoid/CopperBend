using CopperBend.Model;
using NUnit.Framework;

namespace CopperBend.Engine.Tests
{
    [TestFixture]
    public class EngineTests
    {
        [Test]
        public void Can_init_ID_generator_service()
        {
            Item.IDGenerator = null;
            Engine.InitializeIDGenerator();
            Assert.That(Item.IDGenerator, Is.Not.Null);
        }

        [Test]
        public void Can_init_plant_repos()
        {
            Seed.PlantByName = null;
            Engine.InitializePlantRepos();
            Assert.That(Seed.PlantByName, Is.Not.Null);
        }
    }
}
