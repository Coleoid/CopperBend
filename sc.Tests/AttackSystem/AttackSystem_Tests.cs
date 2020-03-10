using System.Collections.Generic;
using System.Linq;
using CopperBend.Fabric;
using CopperBend.Model;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace CopperBend.Logic.Tests
{
    /* === Attack Phases
     *
     * 0. Build attack
     * 0.1. Choose attack and defense methods
     *      multiple 
     *      dodge, parry/deflect  (big soft spot in my design right now)
     *      block, armor
     * 0.2. Resolve those choices plus passives to effects and mods
     * 1. Calc attack
     * 1.1. Apply attack mods
     * 1.2. Roll damage
     * 1.3. Check for triggered effects
     *      (may return us to 1.1 or 1.2)
     * 2. Calc defense
     * 2.1. Apply defense mods
     * 2.2. Apply to attack effects
     * 2.3. Check for triggered effects
     *      (may return us to 2.1 or 2.2)
     * 3. Resolve to a set of effects (damage of different types, ...)
     * 4. post-attack effects
     *    damage
     *    death or destruction? => clean up
     *    incapacitation or other 'status' effects (morale, confusion)
     *    damage over time, other temporary effects...
     *    spend (gain?) resources
     *    attacker and defender gain experience
     *
     */


    // Can offset an attack by a constant
    // Can scale an attack by a ratio
    // Can offset a defense by a constant
    // Can scale a defense by a ratio
    // Can (set|offset) (lower|upper) bound on a defense

    [TestFixture]
    public class AttackSystem_Tests : AttackSystem_TestBase
    {
        [Test]
        public void AAA_Gather_Startup_Costs()
        {
            // Because AttackSystem_Tests is alphabetically first...
            __log.Info("This also seems to gather a bit more init cost.");
        }

        [Test]
        public void Damage_rolls_within_expected_ranges()
        {
            var asys = new AttackSystem(null, __log);
            bool rolled_min = false;
            bool rolled_max = false;
            for (int i = 0; i < 1000; i++)
            {
                int damage = asys.RollDamage(bfh_impact);
                if (damage == 6) rolled_min = true;
                if (damage == 16) rolled_max = true;
                Assert.That(damage, Is.GreaterThanOrEqualTo(6));
                Assert.That(damage, Is.LessThanOrEqualTo(16));
            }

            // Technically a nondeterministic test, so, technically, evil.
            Assert.That(rolled_min, "the probability of not rolling min in 1000 is 5.8e-13, or odds of 1.7 trillion to one.");
            Assert.That(rolled_max, "the probability of not rolling max in 1000 is 5.8e-13, or odds of 1.7 trillion to one.");
        }

        [Test]
        public void Can_resist_a_set_of_AttackDamages()
        {
            var asys = new AttackSystem(null, __log);
            List<AttackDamage> damages = new List<AttackDamage>
            {
                new AttackDamage(8, "physical.impact.blunt"),
                new AttackDamage(6, "physical.impact.edge"),
                new AttackDamage(6, "energetic.fire"),
            };

            Assert.That(damages[0].Initial, Is.EqualTo(8));
            Assert.That(damages[0].Current, Is.EqualTo(8), "initial and current begin equal");

            var out_dmgs = asys.ResistDamages(damages, leather_armor).ToList();

            Assert.That(out_dmgs[0].Initial, Is.EqualTo(8), "resistance doesn't alter initial value");

            Assert.That(out_dmgs[0].Current, Is.EqualTo(6), "leather resists blunt damage poorly");
            Assert.That(out_dmgs[1].Current, Is.EqualTo(3), "leather resists other physical damage better");
            Assert.That(out_dmgs[2].Current, Is.EqualTo(2), "leather resists energy damage well");
        }

        [TestCase(9, "energetic.lightning", 4)]
        [TestCase(9, "magical", 4)]
        [TestCase(9, "vital", 4)]
        [TestCase(9, "sausage", 4)]  //0.2: would be nice if this broke, to rule out typos.
        public void Default_resistance_when_type_has_no_match(int initial, string type, int expected)
        {
            var asys = new AttackSystem(null, __log);
            List<AttackDamage> damages = new List<AttackDamage>
            {
                new AttackDamage(initial, type),
            };

            var out_dmgs = asys.ResistDamages(damages, ring_armor);  //  default for ring is 1/2 ..5

            Assert.That(out_dmgs.First().Current, Is.EqualTo(expected));
        }

        [Test]
        public void Nature_strikes_the_rot_through_our_hero()
        {
            var asys = new AttackSystem(null, __log);

            var player = BeingCreator.CreateBeing("Suvail");
            var am = new AttackMethod("physical.impact.blunt", "1d3 +2");
            var rot = new AreaRot();
            Attack attack = new Attack
            {
                Attacker = player,
                AttackMethod = am,
                Defender = rot,
                DefenseMethod = rot.GetDefenseMethod(am)
            };

            Assert.That(asys.AttackQueue.Count, Is.EqualTo(0));

            asys.RotMap = new RotMap();
            asys.CheckForSpecials(attack);

            Assert.That(asys.AttackQueue.Count, Is.EqualTo(2));

            var splashBack = asys.AttackQueue.Dequeue();
            Assert.NotNull(splashBack);

            var newAttack = asys.AttackQueue.Dequeue();
            var newAM = newAttack.AttackMethod;
            var newAE = newAM.AttackEffects[0];
            Assert.That(newAE.Type, Is.EqualTo("vital.nature.itself"));
            Assert.That(newAttack.Defender, Is.EqualTo(rot));
            Assert.That(newAttack.Attacker, Is.EqualTo(player));
        }

        [Test]
        public void Nature_does_not_strike_the_rot_via_other_sources()
        {
            var asys = new AttackSystem(null, __log);

            var flameRat = BeingCreator.CreateBeing("flame rat");
            var am = new AttackMethod("physical.impact.blunt", "1d3 +2");
            var rot = new AreaRot();
            Attack attack = new Attack
            {
                Attacker = flameRat,
                AttackMethod = am,
                Defender = rot,
                DefenseMethod = rot.GetDefenseMethod(am)
            };

            Assert.That(asys.AttackQueue.Count, Is.EqualTo(0));

            asys.RotMap = new RotMap();
            asys.CheckForSpecials(attack);

            Assert.That(asys.AttackQueue.Count, Is.EqualTo(1));
        }

        [Test]
        public void Nature_strikes_neighboring_rot_through_our_hero()
        {
            var player = BeingCreator.CreateBeing("Suvail");
            var am = new AttackMethod("physical.impact.blunt", "1d3 +2");
            var rot = new AreaRot();
            Attack attack = new Attack
            {
                Attacker = player,
                AttackMethod = am,
                Defender = rot,
                DefenseMethod = rot.GetDefenseMethod(am)
            };

            RotMap rotMap = new RotMap();
            //...add two neighbor ABs, and one further away
            var nbor_1 = new AreaRot();
            var nbor_2 = new AreaRot();
            var stranger = new AreaRot();
            rotMap.Add(rot, (2, 2));
            rotMap.Add(nbor_1, (2, 3));
            rotMap.Add(nbor_2, (3, 1));
            rotMap.Add(stranger, (8, 2));

            var asys = new AttackSystem(null, __log) { RotMap = rotMap };

            Assert.That(asys.AttackQueue.Count, Is.EqualTo(0));

            asys.CheckForSpecials(attack);

            Assert.That(asys.AttackQueue.Count, Is.EqualTo(4));

            var splashBack = asys.AttackQueue.Dequeue();
            Assert.NotNull(splashBack);

            var newAttack = asys.AttackQueue.Dequeue();
            Assert.That(newAttack.Defender, Is.EqualTo(rot));

            newAttack = asys.AttackQueue.Dequeue();
            Assert.That(newAttack.Defender, Is.EqualTo(nbor_2));
            newAttack = asys.AttackQueue.Dequeue();
            Assert.That(newAttack.Defender, Is.EqualTo(nbor_1));
        }
    }
}
