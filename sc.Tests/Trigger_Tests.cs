using CopperBend.Contract;
using NUnit.Framework;
using CopperBend.Fabric.Tests;
using System.Linq;

namespace CopperBend.Logic.Tests
{
    [TestFixture]
    public class Trigger_Tests
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            var __factory = UTHelp.GetSubstituteFactory();
            Engine.Cosmogenesis("seed", __factory);
        }

        [TestCase(0, 0, false)]
        [TestCase(3, 18, false)]
        [TestCase(4, 18, true)]
        [TestCase(5, 20, true)]
        [TestCase(6, 19, true)]
        [TestCase(4, 22, true)]
        [TestCase(4, 23, false)]
        public void LocationTrigger_Range_firing_only_when_location_enclosed(int x, int y, bool shouldFire)
        {
            var t = new Trigger
            {
                Name = "Home on the range",
                Categories = TriggerCategories.PlayerLocation,
                Condition = "(4, 18) to (6, 22)",
                Script = { "Where am I?" },
            };

            var puller = new TriggerPuller();
            bool result = puller.LocationShouldFire((x, y), t);

            Assert.That(result, Is.EqualTo(shouldFire));
        }

        [TestCase(0, 0, false)]
        [TestCase(3, 18, false)]
        [TestCase(4, 18, true)]
        [TestCase(18, 4, false)]
        public void LocationTrigger_firing_only_when_location_matched(int x, int y, bool shouldFire)
        {
            var t = new Trigger
            {
                Name = "Hello, world",
                Categories = TriggerCategories.PlayerLocation,
                Condition = "(4, 18)",
                Script = { "hello", "world" },
            };

            var puller = new TriggerPuller();
            bool result = puller.LocationShouldFire((x, y), t);

            Assert.That(result, Is.EqualTo(shouldFire));
        }

        public (Trigger t, Trigger u, ITriggerHolder holder) GetTestTriggers()
        {
            var t = new Trigger
            {
                Name = "Hello, world",
                Categories = TriggerCategories.PlayerLocation,
                Condition = "(4, 18)",
                Script = { "hello", "world" },
            };

            var u = new Trigger
            {
                Name = "View of the Mine Entrance",
                Categories = TriggerCategories.PlayerLineOfSight,
                Condition = "(22, 18)",
                Script = { "message", "The minehead is grim and dark", "end message" },
            };

            var holder = new TriggerHolderBacking();
            holder.AddTrigger(t);
            holder.AddTrigger(u);

            return (t, u, holder);
        }

        [Test]
        public void Adding_TriggerHolder_to_scope()
        {
            var (_, _, holder) = GetTestTriggers();
            var puller = new TriggerPuller();

            Assert.That(puller.TriggersInScope.Count(), Is.EqualTo(0));
            puller.AddTriggerHolderToScope(holder);
            Assert.That(puller.TriggersInScope.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Removng_TriggerHolder_from_scope()
        {
            var (_, _, holder) = GetTestTriggers();
            var puller = new TriggerPuller();
            puller.AddTriggerHolderToScope(holder);

            Assert.That(puller.TriggersInScope.Count(), Is.EqualTo(2));
            puller.RemoveTriggerHolderFromScope(holder);
            Assert.That(puller.TriggersInScope.Count(), Is.EqualTo(0));
        }

        [Test]
        public void GetLocationTriggers_filters_by_category()
        {
            var (t, _, holder) = GetTestTriggers();
            var puller = new TriggerPuller();
            puller.AddTriggerHolderToScope(holder);

            Assert.That(puller.TriggersInScope.Count(), Is.EqualTo(2));
            var locTriggers = puller.GetLocationTriggers();
            Assert.That(locTriggers.Count, Is.EqualTo(1));
            Assert.That(locTriggers[0], Is.SameAs(t));
        }

        [Test]
        public void HasFlags_PartialOverlap()
        {
            var f1 = TriggerCategories.MapChanged | TriggerCategories.Mortality | TriggerCategories.PlayerLocation;
            var f2 = TriggerCategories.PlayerLocation | TriggerCategories.Mortality;

            Assert.That(f1.HasFlag(f2));
        }
    }
}
