using CopperBend.Contract;
using NUnit.Framework;

namespace CopperBend.Fabric.Tests
{
    [TestFixture]
    public class TomeOfChaos_Tests
    {
        private ISadConEntityFactory __factory;
        private BeingCreator _creator;

        
        [SetUp]
        public void SetUp()
        {
            __factory = UTHelp.GetSubstituteFactory();

            _creator = new BeingCreator(__factory);
        }

        [Test]
        public void TopSeed_is_recorded()
        {
            var tome = new TomeOfChaos();
            tome.SetTopSeed("SampleTopSeed");

            Assert.That(tome.TopSeed, Is.EqualTo("SampleTopSeed"));
        }

        [TestCase("A")]
        [TestCase("B")]
        public void MapSeeds_lead_to_different_values(string topSeed)
        {
            var publisher = new BookPublisher(_creator);
            var tome_1 = publisher.Tome_FromNew(topSeed + "a");
            var tome_2 = publisher.Tome_FromNew(topSeed + "b");

            int next_tf_1 = tome_1.MapRndNext(Maps.TackerFarm);
            int next_tf_2 = tome_2.MapRndNext(Maps.TackerFarm);

            int next_tb_1 = tome_1.MapRndNext(Maps.TownBarricade);
            int next_tb_2 = tome_2.MapRndNext(Maps.TownBarricade);

            Assert.That(next_tb_1, Is.Not.EqualTo(next_tf_1));
            Assert.That(next_tb_2, Is.Not.EqualTo(next_tf_2));
        }

        [TestCase("A")]
        [TestCase("B")]
        public void MapSeeds_remain_stable_when_used_in_different_orders(string topSeed)
        {
            var publisher = new BookPublisher(_creator);
            var tome_1 = publisher.Tome_FromNew(topSeed);
            var tome_2 = publisher.Tome_FromNew(topSeed);

            int next_tf_1 = tome_1.MapRndNext(Maps.TackerFarm);
            int next_tb_1 = tome_1.MapRndNext(Maps.TownBarricade);

            int next_tb_2 = tome_2.MapRndNext(Maps.TownBarricade);
            int next_tf_2 = tome_2.MapRndNext(Maps.TackerFarm);

            Assert.That(next_tb_1, Is.EqualTo(next_tb_2));
            Assert.That(next_tf_1, Is.EqualTo(next_tf_2));
        }

        [TestCase("A")]
        [TestCase("B")]
        public void LearnableSeeds_lead_to_different_values(string topSeed)
        {
            var publisher = new BookPublisher(_creator);
            var tome_1 = publisher.Tome_FromNew(topSeed + "a");
            var tome_2 = publisher.Tome_FromNew(topSeed + "b");

            int next_se_1 = tome_1.LearnableRndNext();
            int next_se_2 = tome_2.LearnableRndNext();

            int next_sc_1 = tome_1.LearnableRndNext();
            int next_sc_2 = tome_2.LearnableRndNext();

            Assert.That(next_sc_1, Is.Not.EqualTo(next_se_1));
            Assert.That(next_sc_2, Is.Not.EqualTo(next_se_2));
        }
    }
}
