using System.Collections.Generic;
using CopperBend.Model;
using CopperBend.Engine;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace sc.Tests
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
    public class AttackSystem_Tests
    {
        AttackMethod tac;
        AttackEffect tac_impact;
        AttackEffect tac_flame;

        AttackMethod bfh;
        AttackEffect bfh_impact;
        AttackEffect bfh_flame;

        DefenseMethod leather_armor;
        DefenseMethod ring_armor;


        [SetUp]
        public void SetUp()
        {
            // Torch as club, crunch and burn
            tac = new AttackMethod();
            tac_impact = new AttackEffect
            {
                Type = "physical.impact.blunt",
                DamageRange = "1d5"
            };
            tac_flame = new AttackEffect
            {
                Type = "energetic.fire",
                DamageRange = "1d3 - 1"
            };
            tac.AttackEffects.Add(tac_impact);
            tac.AttackEffects.Add(tac_flame);

            // Brekka-onu's Flame Hammer, bigger crunch, bigger burn
            bfh = new AttackMethod();
            bfh_impact = new AttackEffect
            {
                Type = "physical.impact.blunt",
                DamageRange = "2d6 + 2"
            };
            bfh_flame = new AttackEffect
            {
                Type = "energetic.fire",
                DamageRange = "1d4 + 2"
            };
            bfh.AttackEffects.Add(bfh_impact);
            bfh.AttackEffects.Add(bfh_flame);

            leather_armor = new DefenseMethod();
            leather_armor.Resistances["physical.impact.blunt"] = "1/4 ..4";
            leather_armor.Resistances["physical"] = "1/2 ..4";
            leather_armor.Resistances["energetic"] = "2/3 ..3";
            leather_armor.Resistances["magical"] = "1/3 ..1";
            leather_armor.Resistances["vital"] = "1/3 ..2";
            //leather_armor.Resistances["default"] = "1/3 ..3";  //not needed with all branches covered

            ring_armor = new DefenseMethod();
            ring_armor.Resistances["physical.impact.blunt"] = "1/2 ..6";
            ring_armor.Resistances["physical"] = "2/3 ..8";
            ring_armor.Resistances["energetic.fire"] = "2/3 ..5";
            ring_armor.Resistances["default"] = "1/2 ..5";

            //0.2: Keep the tree of damage types in data, and type-check attacks/defenses at load time...
        }


        [Test]
        public void Damage_within_expected_ranges()
        {
            var asys = new AttackSystem(null);
            bool rolled_min = false;
            bool rolled_max = false;
            for (int i = 0; i < 1000; i++)
            {
                int damage = asys.RollDamage(bfh_impact);
                if (damage == 4) rolled_min = true;
                if (damage == 14) rolled_max = true;
                Assert.That(damage, Is.GreaterThanOrEqualTo(4));
                Assert.That(damage, Is.LessThanOrEqualTo(14));
            }

            // Technically a nondeterministic test, so, technically, evil.
            Assert.That(rolled_min, "the odds of not rolling min in 1000 are 5.8e-13, or 1.7 trillion to one.");
            Assert.That(rolled_max, "the odds of not rolling max in 1000 are 5.8e-13, or 1.7 trillion to one.");
        }

        [Test]
        public void Can_resist_a_set_of_AttackDamages()
        {
            var asys = new AttackSystem(null);
            List<AttackDamage> damages = new List<AttackDamage>
            {
                new AttackDamage {Initial = 9, Current = 9, DT = "physical.impact.blunt"},
                new AttackDamage {Initial = 5, Current = 5, DT = "energetic.fire"},
                new AttackDamage {Initial = 1, Current = 1, DT = "physical.impact.edge"},
            };

            asys.ResistDamages(damages, leather_armor);

            Assert.That(damages[0].Initial, Is.EqualTo(9));
            Assert.That(damages[0].Current, Is.EqualTo(7));
            Assert.That(damages[1].Initial, Is.EqualTo(5));
            Assert.That(damages[1].Current, Is.EqualTo(2));
            Assert.That(damages[2].Initial, Is.EqualTo(1));
            Assert.That(damages[2].Current, Is.EqualTo(0));
        }

        [Test]
        public void Damage_resistance_when_not_otherwise_specified()
        {
            var asys = new AttackSystem(null);
            List<AttackDamage> damages = new List<AttackDamage>
            {
                new AttackDamage {Initial = 9, Current = 9, DT = "energetic.lightning"},
                new AttackDamage {Initial = 5, Current = 5, DT = "energetic.light"},
                new AttackDamage {Initial = 1, Current = 1, DT = "vital.toxin"},
            };

            asys.ResistDamages(damages, leather_armor);

            Assert.That(damages[0].Initial, Is.EqualTo(9));
            Assert.That(damages[0].Current, Is.EqualTo(6));
            Assert.That(damages[1].Initial, Is.EqualTo(5));
            Assert.That(damages[1].Current, Is.EqualTo(2));
            Assert.That(damages[2].Initial, Is.EqualTo(1));
            Assert.That(damages[2].Current, Is.EqualTo(1));
        }
    }
}
