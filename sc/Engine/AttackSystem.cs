using System;
using System.Collections.Generic;
using GoRogue.DiceNotation;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;

namespace CopperBend.Engine
{
    /*

1.  Build attack
    A.  Choose attack and defense methods
        multiple types of damage possible per attack
        dodge, parry/deflect  (big soft spot in my design right now)
        block (strength + tool)
        armor/resistance

        choosable modifiers due to general skill, special ability, hardware?
            ? from data to UI...
    B.  Resolve those choices to (att|def) effects and (att|def) mods
2. Calc attack
    A.  Apply attack mods
    B.  Roll damage
    C.  Check for triggered effects
        (may return us to 1.1 or 1.2)
3.  Calc defense
    A.  Apply defense mods
    B.  Apply to attack effects
    C.  Check for triggered effects
            May return us to 3.A. or 3.B.
            May create a new attack

4.  Resolve to a set of effects (damage of different types, ...)
5.  Apply post-attack effects
        Register damage (death or destruction? => clean up)
        Time range effects
            'status' effects (stun, fear, confusion)
            damage over time, ...
        Spend (gain?) resources
        attacker and defender gain experience

     */

    public class AttackSystem
    {
        public void Damage(IAttacker attacker, IAttackMethod attack, IDefender defender, IDefenseMethod defense)
        {
            IEnumerable<AttackDamage> damages;
            
            //TODO:  Check if the attacker has any modifiers to the AttackMethod
            //  e.g., Aura of Smite Sauce:  +2 to Impact_blunt, +2 against Unholy
            //  benefits apply after rolling damage?
            //  needs to query defender for 'against' matches
            //  e.g., Rage:  x 1.5 damage, x .75 defense, x 2.5 fatigue
            //  defense debuff applied during resist_damages
            //  fatigue multiplier applied in step 5
            //  ...these go way beyond modifying the AttackMethod.

            // = 2.B. Roll Damage
            damages = RollDamages(attack);
            
            // = 3.B.
            Resist_damages(damages, defense);
        }

        public IEnumerable<AttackDamage> RollDamages(IAttackMethod attack)
        {
            var damages = new List<AttackDamage>();
            foreach (var effect in attack.AttackEffects)
            {
                var roll = Roll_damage(effect);
                var damage = new AttackDamage
                {
                    Type = effect.DamageType,
                    Initial = roll,
                    Current = roll,
                };
                damages.Add(damage);
            }

            return damages;
        }

        public int Roll_damage(IAttackEffect effect)
        {
            return Dice.Roll(effect.DamageRange);
        }

        public void Resist_damages(IEnumerable<AttackDamage> damages, IDefenseMethod defense)
        {
            foreach (var damage in damages)
            {
                if (defense.DamageResistances.ContainsKey(damage.Type))
                {
                    var resistance = defense.DamageResistances[damage.Type];
                    var resisted = new ClampedRatio(resistance).Apply(damage.Current);
                    damage.Current -= Math.Clamp(resisted, 0, damage.Current);
                }
                //TODO:  Add fallback to DamageType.Not_otherwise_specified
            }
        }

    }
}
