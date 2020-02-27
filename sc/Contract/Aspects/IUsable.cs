using System.Collections.Generic;

namespace CopperBend.Contract
{
    public interface IUsable
    {
        string VerbPhrase { get; set; }
        List<UseCost> Costs { get; }
        List<UseEffect> Effects { get; }
        UseTargetFlags Targets { get; set; }

        IUsable AddCosts(params (string substance, int amount)[] costs);
        IUsable AddCost(string substance, int amount);
        IUsable AddEffects(params (string effect, int amount)[] effects);
        IUsable AddEffect(string effect, int amount);
    }

    public struct UseCost
    {
        public UseCost(string substance, int amount)
        {
            Substance = substance;
            Amount = amount;
        }
        public string Substance { get; set; }
        public int Amount { get; set; }
    }

    public struct UseEffect
    {
        public UseEffect(string effect, int amount)
        {
            Effect = effect;
            Amount = amount;
        }
        public string Effect { get; set; }
        public int Amount { get; set; }
    }
}
