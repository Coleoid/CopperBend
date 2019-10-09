using CopperBend.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CopperBend.Model.Aspects
{
    public class Usable : IUsable
    {
        public List<IUse> Uses { get; set; }

        public Usable()
        {
            Uses = new List<IUse>();
        }

        public Usable(params Use[] uses)
            : this()
        {
            Uses.AddRange(uses);
        }

        public Usable AddUse(Use use)
        {
            Uses.Add(use);
            return this;
        }
    }

    public class Use : IUse
    {
        public string Verb { get; set; }

        public List<UseCost> Costs { get; set; }
        public List<UseEffect> Effects { get; set; }
        public UseTargetFlags Targets { get; set; }

        public bool IsExpended
        {
            get => Costs.Any(c => c.Substance == "this");
        }
        public bool TakesDirection 
        {
            get => Targets.HasFlag(UseTargetFlags.Direction);
        }

        public Use(string verb, UseTargetFlags targets)
        {
            Verb = verb;
            Targets = targets;
            Costs = new List<UseCost>();
            Effects = new List<UseEffect>();
        }

        public Use AddCosts(params (string substance, int amount)[] costs)
        {
            foreach ((string substance, int amount) in costs)
                Costs.Add(new UseCost(substance, amount));
            return this;
        }

        public Use AddCost(string substance, int amount)
        {
            Costs.Add(new UseCost(substance, amount));
            return this;
        }

        public Use AddEffects(params (string effect, int amount)[] effects)
        {
            foreach ((string effect, int amount) in effects)
                Effects.Add(new UseEffect(effect, amount));
            return this;
        }

        public Use AddEffect(string effect, int amount)
        {
            Effects.Add(new UseEffect(effect, amount));
            return this;
        }
    }
}
