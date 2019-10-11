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
            var food = new Usable("eat", UseTargetFlags.Self)
                .AddEffect("food", 240)
                .AddCosts(("this", 1), ("time", 16));

            Assert.That(food.VerbPhrase, Is.EqualTo("eat"));
            Assert.That(food.Targets, Is.EqualTo(UseTargetFlags.Self));

            Assert.That(food.Effects.Count, Is.EqualTo(1));
            var effect = food.Effects[0];
            Assert.That(effect.Effect, Is.EqualTo("food"));
            Assert.That(effect.Amount, Is.EqualTo(240));

            Assert.That(food.Costs.Count, Is.EqualTo(2));
            var cost = food.Costs[0];
            Assert.That(cost.Substance, Is.EqualTo("this"));
            Assert.That(cost.Amount, Is.EqualTo(1));
            cost = food.Costs[1];
            Assert.That(cost.Substance, Is.EqualTo("time"));
            Assert.That(cost.Amount, Is.EqualTo(16));
        }

        [Test]
        public void Usable_can_express_our_needs()
        {
            var hoe = new Item((0, 0));
            hoe.AddAspect(new Usable("till ground with", UseTargetFlags.Direction)
                .AddEffect("till", 1)
                .AddCosts(("time", 24), ("energy", 20))
            );
            hoe.AddAspect(new Usable("attack with", UseTargetFlags.Direction)
                .AddEffect("attack", 1)
                .AddCosts(("time", 24), ("energy", 20)));
        }

        [Test]
        public void Weapon_is_Usable_plus_Attack_stuff()
        {

        }
    }
}
