using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.DiceNotation;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;
using log4net;

namespace CopperBend.Logic
{

    /*

Build attack
    Choose attack method
        0.1: Currently only single attack from Wielded tool
            Completely inadequate for creatures
            Eventually inadequate for tool-users
        Filtered by affordable (somehow in UI)
        Multiple types of damage possible per attack
        ? Choosable (in UI) modifiers
            Competency, special ability, gear, ...?
            How does this come from data through the UI?
        Pay resource costs

    Choose defense method  (missing from my design 30 Sep 19)
        Filtered by affordable
        Dodge, parry/deflect
        Block (strength + tool)
        Armor or similar resistance
        Pay resource costs

    Queue the attack  //  QueueAttack(a, am, d, dm)


For each queued attack  //  ResolveAttackQueue()

    Probably cancel if defender destroyed
    Perhaps cancel if attacker destroyed

    Calc attack
        Apply attack mods
        Roll damage
        Check for triggered effects
            May add more Attacks to the queue

    Calc defense
        Apply dodge/deflect/nullify defenses
        Apply resistance/armor to type of attack effect
        Apply to attack effects
        Check for triggered effects
            May add more Attacks to the queue

    Resolve effects (damage of different types, ...)

        Directly adjust values
            Accumulate time costs
            Experience for attacker and defender
            Conciousness, nerve, fatigue, ...
            Health (may add target to Destroyed list)

        Apply extended effects
            Status effects (stun, fear, haste, ...)
            Damage over time (burn, bleed, tox, ...)

Apply post-attack effects

    Convert Destroyed list
        Remove from schedule
        Remove from being, rot, or item map
        Drop items (roll for loot?)
        Check story line / quest triggers

    */

    public class AttackSystem : IAttackSystem
    {
        [InjectProperty] private ILog Log { get; set; }
        [InjectProperty] public IControlPanel Panel { get; set; }

        [InjectProperty] public IGameState GameState { get; set; }
        public IRotMap RotMap { get => GameState.Map.RotMap; }

        public Queue<IDelible> Destroyed { get; }
        public Queue<IAttack> AttackQueue { get; }


        public AttackSystem()
        {
            Destroyed = new Queue<IDelible>();
            AttackQueue = new Queue<IAttack>();
        }

        public void AddAttack(IAttacker attacker, IAttackMethod attack, IDefender defender, IDefenseMethod defense)
        {
            AddAttack(new Attack
            {
                Attacker = attacker,
                AttackMethod = attack,
                Defender = defender,
                DefenseMethod = defense,
            });
        }
        public void AddAttack(IAttack attack) => AttackQueue.Enqueue(attack);


        public void ResolveAttackQueue()
        {
            while (AttackQueue.Count > 0)
            {
                ResolveAttack(AttackQueue.Dequeue());
            }

            ReapDestroyed();
        }

        public void ResolveAttack(IAttack attack)
        {
            IEnumerable<AttackDamage> damages;

            CheckForSpecials(attack);

            damages = RollDamages(attack.AttackMethod);

            damages = ResistDamages(damages, attack.DefenseMethod);

            RegisterDamage(attack.Defender, damages);
        }

        /// <summary>
        /// There are only a few odd damage cases, for now, so
        /// stuffing them in a little zoo should keep them (and us) safe.
        /// </summary>
        public void CheckForSpecials(IAttack attack)
        {
            //  Rot splashback
            if (attack.Defender is AreaRot rot &&
                attack.AttackMethod.AttackEffects.Any(ae =>
                ae.Type.StartsWith("physical", StringComparison.InvariantCulture))
            )
            {
                Log.Info("Rot strikeback");
                var newDefender = (IDefender)attack.Attacker;
                var newAM = new AttackMethod("vital.rot.toxin", "3d3");
                AttackQueue.Enqueue(new Attack
                {
                    Attacker = rot,
                    Defender = newDefender,
                    AttackMethod = newAM,
                    DefenseMethod = newDefender.GetDefenseMethod(newAM),
                });
            }

            //  Nature strikes the rot through our hero
            if (attack.Defender is AreaRot areaRot &&
                attack.Attacker is Model.Being being &&
                being.IsPlayer &&
                attack.AttackMethod.AttackEffects.Any(ae =>
                ae.Type.StartsWith("physical", StringComparison.InvariantCulture))
            )
            {
                Log.Info("Nature through our hero");
                var newAM = new AttackMethod("vital.nature.itself", "3d3");
                AttackQueue.Enqueue(new Attack
                {
                    Attacker = being,
                    Defender = areaRot,
                    AttackMethod = newAM,
                    DefenseMethod = areaRot.GetDefenseMethod(newAM),
                });

                foreach (IAreaRot neighborRot in NeighborRotsOf(areaRot))
                {
                    AttackQueue.Enqueue(new Attack
                    {
                        Attacker = being,
                        Defender = neighborRot,
                        AttackMethod = newAM,
                        DefenseMethod = neighborRot.GetDefenseMethod(newAM),
                    });
                }
            }


            //TODO:  Check if the attacker has any modifiers to the AttackMethod
            //  e.g., Aura of Smite Sauce:  +2 to impact.blunt, +2 vs Unholy
            //  * Modifiers apply after rolling damage?
            //  * Query defender for 'vs' matches
            //  e.g., Rage:  x 1.5 damage, x .75 defense, x 2.5 fatigue
            //  * defense debuff applied **during resist_damages**
            //  * fatigue multiplier applied in step 5
            //  ...these go way beyond modifying the AttackMethod.
            //  Time to think some more.
        }

        public IEnumerable<IAreaRot> NeighborRotsOf(IAreaRot areaRot)
        {
            var coord = RotMap.GetPosition(areaRot);
            return RotMap.GetNonNullItems(coord.Neighbors());
        }

        public void RegisterDamage(IDelible target, IEnumerable<AttackDamage> damages)
        {
            int amount = damages.Sum(d => d.Current);
            if (amount < 1) return;

            target.Hurt(amount);

            MessageDamage(target, damages);

            if (target.Health < 1)
            {
                //              🦋
                //  (ツ)_/¯
                //  Is this an angel?
                Destroyed.Enqueue(target);
                Log.Info($"Target {target.Name} destroyed.");

            }
        }

        public void ReapDestroyed()
        {
            while (Destroyed.TryDequeue(out var mote))
            {
                if (mote is IBeing being)
                {
                    if (being.IsPlayer)
                    {
                        Log.Info("Game over, man.");
                        //1.+: Game modes (agent of power, hardcore, savescummer)
                        throw new PlayerDiedException();
                    }

                    //0.1: drop fewer items
                    var items = new List<IItem>(being.Inventory);
                    foreach (var it in items)
                    {
                        being.RemoveFromInventory(it);
                        Panel.PutItemOnMap(it, being.GetPosition());
                    }
                }

                Panel.RemoveFromAppropriateMap(mote);

                // remove from schedule
                if (mote is IScheduleAgent agent)
                    Panel.RemoveFromSchedule(agent);

                //0.0: give fight/kill experience
                //Panel.AddExperience()  //0.1: only works for plants atm
            }
        }


        //TODO: Destruction/kill messages... somewhere
        // The rot burns to a crisp
        // ( My hands | The green sparks ) destroy the rot
        // ( The flames destroy | The arrow destroys ) the rot
        // ( My hands tear | The arrow tears ) the rot apart

        private void MessageDamage(IDelible target, IEnumerable<AttackDamage> damages)
        {
            if (target.Health > 0)
            {
                //Message(attacker, Messages.BarehandRotDamage);
            }
            else
            {
            }
        }

        public string AttackMessage(IAttack attack)
        {
            string message = string.Empty;

            //int attackTotalDamage = attack.AttackMethod.

            //if (attack.Defender.Health )

            return message;
        }


        public IEnumerable<AttackDamage> RollDamages(IAttackMethod attack)
        {
            var damages = new List<AttackDamage>();
            foreach (var effect in attack.AttackEffects)
            {
                var roll = RollDamage(effect);
                var damage = new AttackDamage(roll, effect.Type);
                damages.Add(damage);
            }

            return damages;
        }

        public int RollDamage(IAttackEffect effect)
        {
            return Dice.Roll(effect.DamageRange);
        }

        public IEnumerable<AttackDamage> ResistDamages(IEnumerable<AttackDamage> damages, IDefenseMethod defense)
        {
            Dictionary<string, string> drs = defense?.Resistances;
            if (drs == null)
                drs = new Dictionary<string, string>();

            foreach (var damage in damages)
            {
                //  Hunt resistance, successively generalizing by trimming ending
                //  E.g., physical.impact.blunt -> physical.impact -> physical -> default -> 0
                var foundResistance = string.Empty;
                for (
                        string damagePath = damage.Type;
                        damagePath.Length > 0;
                        damagePath = damagePath.Substring(0, damagePath.LastIndexOf('.'))
                    )
                {
                    //  Found it directly?
                    if (drs.TryGetValue(damagePath, out foundResistance)) break;

                    //  If we can't break it down further, it's default or nothing
                    if (damagePath.IndexOf('.') < 0)
                    {
                        drs.TryGetValue("default", out foundResistance);
                        break;
                    }
                }
                if (foundResistance.Length == 0) continue;

                var resisted = new ClampedRatio(foundResistance).Apply(damage.Current);

                //  Resistance can't make the damage worse, or cause healing
                damage.Current -= Math.Clamp(resisted, 0, damage.Current);
            }

            return damages;
        }
    }
}
