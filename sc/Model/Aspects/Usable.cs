using System.Collections.Generic;
using System.Linq;
using CopperBend.Contract;

namespace CopperBend.Model.Aspects
{
    /// <summary> Things which the player (and others) can use. </summary>
    /// <remarks>
    /// They present action options via UI, to the player.
    /// Undecided how to communicate these options to
    /// software agents, so e.g., a clever enemy could drink a potion.
    /// </remarks>
    public class Usable : IUsable
    {
        public string VerbPhrase { get; set; }
        public List<UseCost> Costs { get; set; }
        public List<UseEffect> Effects { get; set; }
        public UseTargetFlags Targets { get; set; }

        public Usable(string verb, UseTargetFlags targets)
        {
            VerbPhrase = verb;
            Targets = targets;
            Costs = new List<UseCost>();
            Effects = new List<UseEffect>();
        }

        public IUsable AddCosts(params (string substance, int amount)[] costs)
        {
            foreach ((string substance, int amount) in costs)
                Costs.Add(new UseCost(substance, amount));
            return this;
        }

        public IUsable AddCost(string substance, int amount)
        {
            Costs.Add(new UseCost(substance, amount));
            return this;
        }

        public IUsable AddEffects(params (string effect, int amount)[] effects)
        {
            foreach ((string effect, int amount) in effects)
                Effects.Add(new UseEffect(effect, amount));
            return this;
        }

        public IUsable AddEffect(string effect, int amount)
        {
            Effects.Add(new UseEffect(effect, amount));
            return this;
        }
    }

    public class Weapon : Usable
    {
        public AttackMethod AttackMethod { get; set; }

        public Weapon(string verbPhrase = "attack with", UseTargetFlags targets = UseTargetFlags.Direction)
            : base(verbPhrase, targets)
        {
            AttackMethod = new AttackMethod();
        }

        public Weapon AddAttackEffects(params (string type, string range)[] effects)
        {
            foreach ((string type, string range) in effects)
                AttackMethod.AddEffect(type, range);
            return this;
        }

        public Weapon AddAttackEffect(string type, string range)
        {
            AttackMethod.AddEffect(type, range);
            return this;
        }
    }
}
