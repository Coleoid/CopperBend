using CopperBend.Contract;
using NUnit.Framework;

namespace CopperBend.Model.Aspects.Tests
{
    [TestFixture]
    public class Usable_Tests
    {
        [Test]
        public void Usable_Construction_not_Insane()
        {
            var food = new Usable(
                new Use("eat", UseTargetFlags.Self)
                    .AddEffect("food", 240)
                    .AddCosts(("this", 1), ("time", 16))
            );

            Assert.That(food.Uses.Count, Is.EqualTo(1));
            var use = food.Uses[0];
            Assert.That(use.Verb, Is.EqualTo("eat"));
            Assert.That(use.Targets, Is.EqualTo(UseTargetFlags.Self));

            Assert.That(use.Effects.Count, Is.EqualTo(1));
            var effect = use.Effects[0];
            Assert.That(effect.Effect, Is.EqualTo("food"));
            Assert.That(effect.Amount, Is.EqualTo(240));

            Assert.That(use.Costs.Count, Is.EqualTo(2));
            var cost = use.Costs[0];
            Assert.That(cost.Substance, Is.EqualTo("this"));
            Assert.That(cost.Amount, Is.EqualTo(1));
            cost = use.Costs[1];
            Assert.That(cost.Substance, Is.EqualTo("time"));
            Assert.That(cost.Amount, Is.EqualTo(16));
        }
    }
}
