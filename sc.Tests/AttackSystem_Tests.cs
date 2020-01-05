using System.Collections.Generic;
using System.Linq;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;
using log4net;
using Microsoft.Xna.Framework;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace CopperBend.Engine.Tests
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

        ILog __log; 

        [SetUp]
        public void SetUp()
        {
            __log = Substitute.For<ILog>();
            Being.IDGenerator = new GoRogue.IDGenerator();
            Being.EntityFactory = Substitute.For<IEntityFactory>();
            AreaBlight.IDGenerator = Being.IDGenerator;

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
                DamageRange = "2d6 + 4"
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
            leather_armor.Resistances["energetic"] = "2/3 ..4";
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

        // Note, our hero is strong against all vital.blight damage.
        [TestCase("physical.impact.point", true)]
        [TestCase("physical.impact.blunt", true)]
        [TestCase("energetic.fire", false)]
        public void Blight_splashback(string damageType, bool willSplashBack)
        {
            // Anyone directly physically assaulting AreaBlight is 
            // hit with immediate vital.blight.toxin damage.
            //0.2: ranged physical damage should avoid splashback.
            var asys = new AttackSystem(null, __log);

            var flameRat = new Being(Color.Red, Color.Black, 'r');
            var am = new AttackMethod(damageType, "1d3 +2");
            var blight = new AreaBlight();
            Attack attack = new Attack
            {
                Attacker = flameRat,
                AttackMethod = am,
                Defender = blight,
                DefenseMethod = blight.GetDefenseMethod(am)
            };

            Assert.That(asys.AttackQueue.Count, Is.EqualTo(0));

            asys.CheckForSpecials(attack);

            int newAttackCount = willSplashBack ? 1 : 0;
            Assert.That(asys.AttackQueue.Count, Is.EqualTo(newAttackCount));
            if (!willSplashBack) return;

            var newAttack = asys.AttackQueue.Dequeue();
            var newAM = newAttack.AttackMethod;
            var newAE = newAM.AttackEffects[0];
            Assert.That(newAE.Type, Is.EqualTo("vital.blight.toxin"));
            Assert.That(newAttack.Defender, Is.EqualTo(flameRat));
            Assert.That(newAttack.Attacker, Is.EqualTo(blight));
        }

        [Test]
        public void Nature_strikes_the_blight_through_our_hero()
        {
            var asys = new AttackSystem(null, __log);

            var player = new Being(Color.LawnGreen, Color.Black, '@');
            player.IsPlayer = true;
            var am = new AttackMethod("physical.impact.blunt", "1d3 +2");
            var blight = new AreaBlight();
            Attack attack = new Attack
            {
                Attacker = player,
                AttackMethod = am,
                Defender = blight,
                DefenseMethod = blight.GetDefenseMethod(am)
            };

            Assert.That(asys.AttackQueue.Count, Is.EqualTo(0));

            asys.BlightMap = new BlightMap();
            asys.CheckForSpecials(attack);

            Assert.That(asys.AttackQueue.Count, Is.EqualTo(2));

            var splashBack = asys.AttackQueue.Dequeue();
            var newAttack = asys.AttackQueue.Dequeue();
            var newAM = newAttack.AttackMethod;
            var newAE = newAM.AttackEffects[0];
            Assert.That(newAE.Type, Is.EqualTo("vital.nature.itself"));
            Assert.That(newAttack.Defender, Is.EqualTo(blight));
            Assert.That(newAttack.Attacker, Is.EqualTo(player));
        }

        [Test]
        public void Nature_strikes_neighboring_blight_through_our_hero()
        {

            var player = new Being(Color.LawnGreen, Color.Black, '@');
            player.IsPlayer = true;
            var am = new AttackMethod("physical.impact.blunt", "1d3 +2");
            var blight = new AreaBlight();
            Attack attack = new Attack
            {
                Attacker = player,
                AttackMethod = am,
                Defender = blight,
                DefenseMethod = blight.GetDefenseMethod(am)
            };

            BlightMap blightMap = new BlightMap();
            //...add two neighbor ABs, and one further away
            var nbor_1 = new AreaBlight();
            var nbor_2 = new AreaBlight();
            var stranger = new AreaBlight();
            blightMap.Add(blight, (2, 2));
            blightMap.Add(nbor_1, (2, 3));
            blightMap.Add(nbor_2, (3, 1));
            blightMap.Add(stranger, (8, 2));

            var asys = new AttackSystem(null, __log);
            asys.BlightMap = blightMap;

            Assert.That(asys.AttackQueue.Count, Is.EqualTo(0));

            asys.CheckForSpecials(attack);

            Assert.That(asys.AttackQueue.Count, Is.EqualTo(4));

            var splashBack = asys.AttackQueue.Dequeue();
            var newAttack = asys.AttackQueue.Dequeue();
            Assert.That(newAttack.Defender, Is.EqualTo(blight));

            newAttack = asys.AttackQueue.Dequeue();
            Assert.That(newAttack.Defender, Is.EqualTo(nbor_2));
            newAttack = asys.AttackQueue.Dequeue();
            Assert.That(newAttack.Defender, Is.EqualTo(nbor_1));
        }
    }
}
