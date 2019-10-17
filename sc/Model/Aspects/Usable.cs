﻿using System.Collections.Generic;
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
