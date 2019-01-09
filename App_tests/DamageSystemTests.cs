using System;
using CopperBend.App.Model;
using CopperBend.MapUtil;
using NUnit.Framework;

namespace CopperBend.App.tests
{
    public class DamageSystem
    {
        //public int ApplyDamage(IActor actor, IDamage damage)
        //{
        //    var defense = actor.DefenseAspect;
        //    var blocked = (int) Math.Ceiling(defense.PortionBlocked * damage.Quantity);
        //    var done = damage.Quantity - blocked;
        //    actor.Hurt(done);
        //    return done;
        //}
    }

    [TestFixture]
    public class DamageSystemTests
    {
        public IActor CreateActor(int startingHealth = 100, int x = 0, int y = 0)
        {
            Point point = new Point(x, y);
            var def = new DefenseAspect();
            var actor = new Actor(point) { DefenseAspect = def, Health = startingHealth };
            def.Actor = actor;

            return actor;
        }

        [TestCase(3, 0, 3)]
        [TestCase(3, 1, 2)]
        public void Damage_comes_off_of_health(int startingHealth, int amount, int expectedHealth)
        {
            var actor = CreateActor(startingHealth);
            var damage = new Damage {Quantity = amount, DamageType = "Sharp"};

            actor.DefenseAspect.ApplyDamage(damage);

            Assert.That(actor.Health, Is.EqualTo(expectedHealth));
        }

        [TestCase(10, 10, 0, 0)]
        [TestCase(10, 10, .5, 5)]
        [TestCase(10, 10, 1, 10)]
        [TestCase(3, 2, .01, 2)]
        public void Defense_can_reduce_damage(int startingHealth, int amount, double portionBlocked, int expectedHealth)
        {
            var actor = CreateActor(startingHealth);
            actor.DefenseAspect.SetResistance("General", portionBlocked);

            var damage = new Damage { Quantity = amount, DamageType = "Sharp" };
            actor.DefenseAspect.ApplyDamage(damage);

            Assert.That(actor.Health, Is.EqualTo(expectedHealth));
        }

        [TestCase(10, "Sharp", 10)]
        [TestCase(10, "Fire", 3)]
        public void Defender_resistance_can_depend_on_damage_type(int amount, string damageType, int expectedDamage)
        {
            var actor = CreateActor();
            actor.DefenseAspect.SetResistance("Fire", .67);  // Vulcan's Favor, two thirds of fire damage is blocked

            var damage = new Damage { Quantity = amount, DamageType = damageType };
            int damageDone = actor.DefenseAspect.ApplyDamage(damage);

            Assert.That(damageDone, Is.EqualTo(expectedDamage));
            Assert.That(actor.Health, Is.EqualTo(100 - damageDone));
        }
    }
}
