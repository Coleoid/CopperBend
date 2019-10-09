using System.Collections.Generic;

namespace CopperBend.Contract
{
    //  This should be enough for much of the basic structure.
    //  Remaining questions:
    //  Does the RNG come from the attacker?  Is it supplied by the AttackSystem?
    //  Do the individual IAttacker/IDefenders decide which effectss to apply, or do
    // they simply send their whole list?
    //  I expect we want the combo/filter logic outside of the combatants.

    public interface IAttacker
    {
        IAttackMethod GetAttackMethod(IDefender defender);
        List<IModifier> GetAttackModifiers(IDefender defender, IAttackMethod method);
    }

    public interface IDefender : IDelible
    {
        IDefenseMethod GetDefenseMethod(IAttackMethod method);
        List<IModifier> GetDefenseModifiers(IAttacker attacker, IAttackMethod method);
    }

    public interface IAttackMethod
    {
        List<IAttackEffect> AttackEffects { get; set; }
        List<IModifier> AttackModifiers { get; set; }
        void AddEffect(IAttackEffect effect);
    }

    public interface IDefenseMethod
    {
        Dictionary<string, string> Resistances { get; set; }
    }

    public interface IAttackEffect
    {
        string Type { get; set; }
        string DamageRange { get; set; }
    }

    public interface IModifier
    {
        /// <summary>
        /// Some modifiers are expended, like "blocks a total of 80 damage" or
        /// "Sharpness enchantment wears off after five hits".  Used modifiers
        /// get WasUsed(amount) calls that they do what they want with.
        /// </summary>
        /// <param name="amount">how much effect the modifier had</param>
        void WasUsed(int amount);
    }
}
