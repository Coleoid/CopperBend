using System.Collections.Generic;
using System.Linq;
using CopperBend.Contract;

namespace CopperBend.Model.Aspects
{
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

        public Usable AddCosts(params (string substance, int amount)[] costs)
        {
            foreach ((string substance, int amount) in costs)
                Costs.Add(new UseCost(substance, amount));
            return this;
        }

        public Usable AddCost(string substance, int amount)
        {
            Costs.Add(new UseCost(substance, amount));
            return this;
        }

        public Usable AddEffects(params (string effect, int amount)[] effects)
        {
            foreach ((string effect, int amount) in effects)
                Effects.Add(new UseEffect(effect, amount));
            return this;
        }

        public Usable AddEffect(string effect, int amount)
        {
            Effects.Add(new UseEffect(effect, amount));
            return this;
        }

        public bool IsExpended
        {
            get => Costs.Any(c => c.Substance == "this");
        }
        public bool TakesDirection
        {
            get => Targets.HasFlag(UseTargetFlags.Direction);
        }
    }
}
