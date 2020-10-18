using CopperBend.Contract;
using CopperBend.Creation;
using CopperBend.Logic.Tests;
using NUnit.Framework;

namespace CopperBend.Fabric.Tests
{
    [TestFixture]
    public class TomeOfChaos_Tests : Tests_Base
    {
        protected override bool ShouldPrepDI => true;
        protected override MockableServices GetServicesToMock()
        {
            return MockableServices.EntityFactory
                | base.GetServicesToMock();
        }

        private BookPublisher _publisher;
        
        [SetUp]
        public void SetUp()
        {
            _publisher = SourceMe.The<BookPublisher>();
        }

        [Test]
        public void TopSeed_is_recorded()
        {
            TomeOfChaos tome = new TomeOfChaos("SampleTopSeed");

            Assert.That(tome.TopSeed, Is.EqualTo("SampleTopSeed"));
        }

        [TestCase("A")]
        [TestCase("B")]
        public void MapSeeds_lead_to_different_values(string topSeed)
        {
            var tome_1 = _publisher.Tome_FromNew(topSeed + "a");
            var tome_2 = _publisher.Tome_FromNew(topSeed + "b");

            int next_tf_1 = tome_1.MapRndNext(MapEnum.TackerFarm);
            int next_tf_2 = tome_2.MapRndNext(MapEnum.TackerFarm);

            int next_tb_1 = tome_1.MapRndNext(MapEnum.TownBarricade);
            int next_tb_2 = tome_2.MapRndNext(MapEnum.TownBarricade);

            Assert.That(next_tb_1, Is.Not.EqualTo(next_tf_1));
            Assert.That(next_tb_2, Is.Not.EqualTo(next_tf_2));
        }

        [TestCase("A")]
        [TestCase("B")]
        public void MapSeeds_remain_stable_when_used_in_different_orders(string topSeed)
        {
            var tome_1 = _publisher.Tome_FromNew(topSeed);
            var tome_2 = _publisher.Tome_FromNew(topSeed);

            int next_tf_1 = tome_1.MapRndNext(MapEnum.TackerFarm);
            int next_tb_1 = tome_1.MapRndNext(MapEnum.TownBarricade);

            int next_tb_2 = tome_2.MapRndNext(MapEnum.TownBarricade);
            int next_tf_2 = tome_2.MapRndNext(MapEnum.TackerFarm);

            Assert.That(next_tb_1, Is.EqualTo(next_tb_2));
            Assert.That(next_tf_1, Is.EqualTo(next_tf_2));
        }

        [TestCase("A")]
        [TestCase("B")]
        public void LearnableSeeds_lead_to_different_values(string topSeed)
        {
            var tome_1 = _publisher.Tome_FromNew(topSeed + "a");
            var tome_2 = _publisher.Tome_FromNew(topSeed + "b");

            int next_se_1 = tome_1.LearnableRndNext();
            int next_se_2 = tome_2.LearnableRndNext();

            int next_sc_1 = tome_1.LearnableRndNext();
            int next_sc_2 = tome_2.LearnableRndNext();

            Assert.That(next_sc_1, Is.Not.EqualTo(next_se_1));
            Assert.That(next_sc_2, Is.Not.EqualTo(next_se_2));
        }
    }
}
