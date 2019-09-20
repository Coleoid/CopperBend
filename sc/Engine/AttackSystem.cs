using CopperBend.Fabric;
using CopperBend.Model;
using GoRogue.DiceNotation;
using System;
using System.Collections.Generic;
using System.Text;

namespace CopperBend.Engine
{
    public class AttackSystem
    {

        public int Roll_damage(AttackEffect effect)
        {
            return Dice.Roll(effect.DamageRange);
        }

        public void Resist_damages(IEnumerable<AttackDamage> damages, DefenseMethod defense)
        {
            foreach (var damage in damages)
            {
                if (defense.DamageResistances.ContainsKey(damage.Type))
                {
                    var resistance = defense.DamageResistances[damage.Type];
                    var resisted = new ClampedRatio(resistance).Apply(damage.Current);
                    damage.Current -= Math.Clamp(resisted, 0, damage.Current);
                }
            }
        }

    }
}
