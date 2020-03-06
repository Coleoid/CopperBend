using CopperBend.Contract;
using CopperBend.Model;
using NSubstitute;
using NUnit.Framework;

namespace CopperBend.Engine.Tests
{
    [TestFixture]
    public class Engine_Tests
    {
        [Test]
        public void Cosmogenesis_sets_ID_generator_service()
        {
            Item.SetIDGenerator(null);
            AreaRot.SetIDGenerator(null);
            Engine.Cosmogenesis("seed", Substitute.For<ISadConEntityFactory>());

            var item = new Item((2, 2));
            var rot = new AreaRot();

            Assert.That(item, Is.Not.Null);
            Assert.That(rot, Is.Not.Null);
            Assert.That(item.ID, Is.EqualTo(rot.ID - 1));
        }

        //[Test]
        //public void Can_init_plant_repos()
        //{
        //    Seed.Herbal = null;
        //    var herbal = Engine.InitHerbal();
        //    Engine.ConnectHerbal(herbal);
        //    Assert.That(Seed.Herbal, Is.SameAs(herbal));
        //}
    }
}
