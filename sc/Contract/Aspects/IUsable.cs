using System;
using System.Collections.Generic;

namespace CopperBend.Contract
{
    /// <summary> Highly speculative </summary>
    public interface IUsable
    {
        string VerbPhrase { get; set; }
        List<UseCost> Costs { get; set; }
        List<UseEffect> Effects { get; set; }
        UseTargetFlags Targets { get; set; }

        IUsable AddCosts(params (string substance, int amount)[] costs);
        IUsable AddCost(string substance, int amount);
        IUsable AddEffects(params (string effect, int amount)[] effects);
        IUsable AddEffect(string effect, int amount);


        bool IsExpended { get; }
        bool TakesDirection { get; }
    }

    public struct UseCost
    {
        public string Substance { get; set; }
        public int Amount { get; set; }
        public UseCost(string substance, int amount)
        {
            Substance = substance;
            Amount = amount;
        }
    }

    public struct UseEffect
    {
        public string Effect { get; set; }
        public int Amount { get; set; }
        public UseEffect(string effect, int amount)
        {
            Effect = effect;
            Amount = amount;
        }
    }

    [Flags]
    public enum UseTargetFlags
    {
        Unset = 0,
        Self = 1,
        Being = 2,
        Direction = 4,
        Location = 8,
        Item = 16,
    }
}
