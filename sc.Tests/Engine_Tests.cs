using CopperBend.Model;
using NUnit.Framework;

namespace CopperBend.Engine.Tests
{
    [TestFixture]
    public class Engine_Tests
    {
        [Test]
        public void Can_init_ID_generator_service()
        {
            Item.IDGenerator = null;
            Engine.Cosmogenesis("seed");
            Assert.That(Item.IDGenerator, Is.Not.Null);
        }

        [Test]
        public void Can_init_plant_repos()
        {
            Seed.Herbal = null;
            Engine.ConnectHerbal();
            Assert.That(Seed.Herbal, Is.Not.Null);
        }
    }
}
