using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace sc.Tests
{
    [TestFixture]
    public class AttackSystem_Tests
    {
        [Test]
        public void Can_block_fraction()
        {
            //if (!Debugger.IsAttached) Debugger.Launch();
            int raw_damage = 9;

            string block_fraction = "1/3";
            int taken = BlockFraction(raw_damage, block_fraction);
            Assert.That(taken, Is.EqualTo(6));
        }

        [Test]
        public void Can_block_fraction_with_max()
        {
            //if (!Debugger.IsAttached) Debugger.Launch();
            int raw_damage = 9;

            string block_fraction = "1/3 max 2";
            int taken = BlockFraction(raw_damage, block_fraction);
            Assert.That(taken, Is.EqualTo(7));
        }

        //0.1.DMG  Handles fraction and max, yet excessively fragile
        private int BlockFraction(int raw_damage, string block_fraction)
        {
            var match = Regex.Match(block_fraction, @"(\d+)/(\d+)(?: max (\d+))?");
            double num = double.Parse(match.Groups[1].Value);
            double den = double.Parse(match.Groups[2].Value);
            double frac = num / den;

            int blocked = (int) Math.Round(raw_damage * frac);
            if (!string.IsNullOrEmpty(match.Groups[3].Value))
            {
                int max = int.Parse(match.Groups[3].Value);
                blocked = Math.Min(blocked, max);
            }

            return raw_damage - blocked;
        }

        /*
         * 1. Apply pre-attack buffs
         * 2. Apply defensive methods
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
    }
}
