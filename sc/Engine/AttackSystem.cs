using System;
using System.Collections.Generic;
using GoRogue.DiceNotation;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;
using System.Linq;

namespace CopperBend.Engine
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
        Remove from being, blight, or item map
        Drop items (roll for loot?)
        Check story line / quest triggers

    */

    /*
    Another view:


    */

    public class Attack
    {
        public IAttacker Attacker { get; set; }
        public IAttackMethod AttackMethod { get; set; }
        public IDefender Defender { get; set; }
        public IDefenseMethod DefenseMethod { get; set; }
    }

    public class AttackSystem
    {
        public AttackSystem(IControlPanel panel)
        {
            Panel = panel;
            Destroyed = new Queue<IDelible>();
            AttackQueue = new Queue<Attack>();
        }

        public IControlPanel Panel { get; set; }
        public Queue<IDelible> Destroyed { get; set; }
        public Queue<Attack> AttackQueue { get; set; }

        public void AddAttack(IAttacker attacker, IAttackMethod attack, IDefender defender, IDefenseMethod defense)
        {
            AddAttack(new Attack {
                Attacker = attacker,
                AttackMethod = attack,
                Defender = defender,
                DefenseMethod = defense
            });
        }
        public void AddAttack(Attack attack) => AttackQueue.Enqueue(attack);

        
        public void ResolveAttackQueue()
        {
            while (AttackQueue.Count() > 0)
            {
                ResolveAttack(AttackQueue.Dequeue());
            }

            ReapDestroyed();
        }

        public void ResolveAttack(Attack attack)
        {
            IEnumerable<AttackDamage> damages;

            CheckForSpecials(attack);

            // = 2.B. Roll Damage
            damages = RollDamages(attack.AttackMethod);

            // = 3.B.
            ResistDamages(damages, attack.DefenseMethod);

            // = 5.A.
            RegisterDamage(attack.Defender, damages);
        }

        public void CheckForSpecials(Attack attack)
        {
            //  Blight splashback
            if (attack.Defender is AreaBlight &&
                attack.AttackMethod.AttackEffects.Any(ae => 
                ae.Type.StartsWith("physical"))
            )
            {
                var newAttacker = (IAttacker)attack.Defender;
                var newDefender = (IDefender)attack.Attacker;
                var newAM = new AttackMethod("vital.blight.toxin", "3d3");
                AttackQueue.Enqueue(new Attack
                {
                    Attacker = newAttacker,
                    Defender = newDefender,
                    AttackMethod = newAM,
                    DefenseMethod = newDefender.GetDefenseMethod(newAM)
                });
            }

            //TODO:  Check if the attacker has any modifiers to the AttackMethod
            //  e.g., Aura of Smite Sauce:  +2 to Impact_blunt, +2 against Unholy
            //  benefits apply after rolling damage?
            //  needs to query defender for 'against' matches
            //  e.g., Rage:  x 1.5 damage, x .75 defense, x 2.5 fatigue
            //  defense debuff applied during resist_damages
            //  fatigue multiplier applied in step 5
            //  ...these go way beyond modifying the AttackMethod.  Time to think again.



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
                        //1.+: Game modes (agent of power, hardcore, savescummer)
                        Panel.WriteLine("I die.");
                        Panel.More();
                        Panel.GameOver(being);
                        return;
                    }

                    //0.1: drop fewer items
                    foreach (var it in being.Inventory)
                    {
                        being.RemoveFromInventory(it);
                        Panel.PutItemOnMap(it, being.Position);
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
        // The blight burns to a crisp
        // ( My hands | The green sparks ) destroy the blight
        // ( The flames destroy | The arrow destroys ) the blight
        // ( My hands tear | The arrow tears ) the blight apart

        private void MessageDamage(IDelible target, IEnumerable<AttackDamage> damages)
        {
            if (target.Health > 0)
            {
                //Message(attacker, Messages.BarehandBlightDamage);
            }
            else
            {
            }
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

        public void ResistDamages(IEnumerable<AttackDamage> damages, IDefenseMethod defense)
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
                if (foundResistance == string.Empty) continue;

                var resisted = new ClampedRatio(foundResistance).Apply(damage.Current);

                //  Resistance can't make the damage worse, or cause healing
                damage.Current -= Math.Clamp(resisted, 0, damage.Current);
            }
        }

        #region Messages

        Dictionary<Messages, bool> SeenMessages { get; set; } = new Dictionary<Messages, bool>();
        /// <summary> First time running across this message in this game run? </summary>
        public bool FirstTimeFor(Messages key)
        {
            var firstTime = !SeenMessages.ContainsKey(key);
            if (firstTime)
                SeenMessages.Add(key, true);

            return firstTime;
        }

        /// <summary>
        /// This allows messages to adapt based on the Being involved and
        /// what messages have already been seen, how many times, et c.
        /// </summary>
        public void Message(IBeing being, Messages messageKey)
        {
            Guard.Against(messageKey == Messages.Unset, "Must set message key");
            if (!being.IsPlayer) return;

            switch (messageKey)
            {
            case Messages.BarehandBlightDamage:
                if (FirstTimeFor(messageKey))
                {
                    //0.2  promote to alert
                    Panel.WriteLine("I tear a chunk off the ground.  It fights back--burns my hands.");
                    Panel.WriteLine("The stuff withers away from where I grab it.");
                }
                else
                {
                    Panel.WriteLine("I hit it, and the stuff withers.");
                }

                break;

            case Messages.BlightDamageSpreads:
                Panel.WriteLine("The damage to this stuff spreads outward.  Good.");
                break;

            default:
                var need_message_for_key = $"Must code message for key [{messageKey}].";
                Panel.WriteLine(need_message_for_key);
                throw new Exception(need_message_for_key);
            }
        }

        #endregion
    }

}
