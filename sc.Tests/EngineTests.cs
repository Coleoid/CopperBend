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
            Engine.InitializeMetaphysics("seed");
            Assert.That(Item.IDGenerator, Is.Not.Null);
        }

        [Test]
        public void Can_init_plant_repos()
        {
            Seed.Herbal = null;
            Engine.InitializePlantRepos();
            Assert.That(Seed.Herbal, Is.Not.Null);
        }
    }
}
