using System.Diagnostics;
using NUnit.Framework;
using NUnit.Framework.Internal;
using CopperBend.Contract;
using CopperBend.Model;

namespace sc.Tests
{
    /* === Attack Phases
     *
     * 0. Build attack
     * 0.1. Choose attack and defense methods
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
     *  dodge, parry/deflect
     *  block, armor
     * 3. Resolve to a set of effects (damage, ...)
     * 4. post-attack effects
     *  apply damage
     *  death, destruction, incapacitation
     *  morale, damage over time, ...
     *  use of resources
     *  experience
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
        [Test]
        public void some()
        {
            // Brekka-onu's Flame Hammer
            var bfh = new AttackMethod();
            var impact = new AttackEffect
            {
                DamageType = DamageType.Impact_blunt,
                DamageRange = "2d5 + 2"
            };
            var flame = new AttackEffect
            {
                DamageType = DamageType.Fire,
                DamageRange = "1d4 + 2"
            };
            bfh.AttackEffects.Add(impact);
            bfh.AttackEffects.Add(flame);

            var leather_armor = new DefenseMethod();
            leather_armor.DamageResistances[DamageType.Impact_blunt] = "1/4 ..4";
            leather_armor.DamageResistances[DamageType.Fire] = "2/3 ..3";

            var ring_armor = new DefenseMethod();
            ring_armor.DamageResistances[DamageType.Impact_blunt] = "1/2 ..6";
            ring_armor.DamageResistances[DamageType.Fire] = "2/3 ..5";
        }
    }
}
