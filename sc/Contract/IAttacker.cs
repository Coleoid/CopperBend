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

    public interface IDefender
    {
        IDefenseMethod GetDefenseMethod(IAttackMethod method);
        List<IModifier> GetDefenseModifiers(IAttacker attacker, IAttackMethod method);
    }

    public interface IAttackMethod
    {
        List<IAttackEffect> AttackEffects { get; set; }
        List<IModifier> AttackModifiers { get; set; }
    }

    public interface IDefenseMethod
    {
        Dictionary<DamageType, string> DamageResistances { get; set; }
    }

    public interface IAttackEffect
    {
        DamageType DamageType { get; set; }
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

    public interface IAttackModifier
    {
        void WasUsed(int amount);
    }

    public interface IDefenseModifier
    {
        void WasUsed(int amount);
    }

    public enum DamageType
    {
        Unset = 0,
        Not_otherwise_specified,  //0.2: Use this type as fallback
        Impact_blunt,
        Impact_edge,
        Impact_point,
        Blight_toxin,
        Nature_plant,
        Fire,
        Lightning,
        Light,
        Water,
    }
    //1.+: Add categories, perhaps physical, energetic, magical, and vital.

}
