using CopperBend.Fabric;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace sc.Tests.Books
{
    [TestFixture]
    public class TomeOfChaos_Tests
    {
        [Test]
        public void TopSeed_is_recorded()
        {
            var tome = new TomeOfChaos("SampleTopSeed");

            Assert.That(tome.TopSeed, Is.EqualTo("SampleTopSeed"));
        }

        [TestCase("A")]
        [TestCase("B")]
        public void MapSeeds_lead_to_different_values(string topSeed)
        {
            var tome_1 = new TomeOfChaos(topSeed + "a");
            var tome_2 = new TomeOfChaos(topSeed + "b");

            int next_tf_1 = tome_1.NextMapValue(MapList.TackerFarm);
            int next_tf_2 = tome_2.NextMapValue(MapList.TackerFarm);

            int next_tb_1 = tome_1.NextMapValue(MapList.TownBastion);
            int next_tb_2 = tome_2.NextMapValue(MapList.TownBastion);

            Assert.That(next_tb_1, Is.Not.EqualTo(next_tf_1));
            Assert.That(next_tb_2, Is.Not.EqualTo(next_tf_2));
        }

        [TestCase("A")]
        [TestCase("B")]
        public void MapSeeds_remain_stable_when_used_in_different_orders(string topSeed)
        {
            var tome_1 = new TomeOfChaos(topSeed);
            var tome_2 = new TomeOfChaos(topSeed);

            int next_tf_1 = tome_1.NextMapValue(MapList.TackerFarm);
            int next_tb_1 = tome_1.NextMapValue(MapList.TownBastion);

            int next_tb_2 = tome_2.NextMapValue(MapList.TownBastion);
            int next_tf_2 = tome_2.NextMapValue(MapList.TackerFarm);

            Assert.That(next_tb_1, Is.EqualTo(next_tb_2));
            Assert.That(next_tf_1, Is.EqualTo(next_tf_2));
        }
    }
}
