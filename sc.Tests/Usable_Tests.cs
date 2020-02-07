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
        public void Usable_can_build_a_hoe()
        {
            var hoe = new Item((0, 0));
            hoe.AddAspect(new Usable("till ground with", UseTargetFlags.Direction)
                .AddEffect("till", 1)
                .AddCosts(("time", 24), ("energy", 20)));
            hoe.AddAspect(new Usable("remove weeds with", UseTargetFlags.Direction)
                .AddEffect("weed", 1)
                .AddCosts(("time", 24), ("energy", 5)));
        }

        [Test]
        public void Usable_can_build_a_pen()
        {
            var pen = new Item((0, 0));
            pen.AddAspect(new Usable("write on object with", UseTargetFlags.Item)
                .AddEffect("write", 1)
                .AddCosts(("time", 20), ("energy", 2)));
        }

        [Test]
        public void Weapon_is_Usable_plus_Attack_stuff()
        {
            var knife = new Item((0, 0));
            var cutTargets = UseTargetFlags.Item | UseTargetFlags.Direction;
            knife.AddAspect(new Usable("cut target with", cutTargets)
                .AddEffect("cut", 4)
                .AddCosts(("time", 6), ("energy", 2)));

            knife.AddAspect(
                new Weapon("attack target with", UseTargetFlags.Being | UseTargetFlags.Direction)
                .AddAttackEffects(
                    ("physical.impact.edge", "1d4+2"),
                    ("physical.impact.blunt", "1d2")
                )
                .AddCosts(("time", 12), ("energy", 8))
            );
        }
    }
}
