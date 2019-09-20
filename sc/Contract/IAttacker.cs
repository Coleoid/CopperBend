using System.Collections.Generic;

namespace CopperBend.Contract
{
    //  This should be enough for much of the basic structure.
    //  Remaining questions:
    //  Does the RNG come from the attacker?  Is it supplied by the AttackSystem?
    //  Do the individual IAttacker/IDefenders decide which effectss to apply, or do
    // they simply send their whole list?
    //  I expect we want the combo/filter logic outside of the combatants.
    //  An effect which lasts a certain number of uses should not be triggered on being
    // supplied, so the WasUsed() is called when resolution determines an effect is
    // active in the attack being resolved.

    public interface IAttacker
    {
        IAttackMethod GetAttackMethod(IDefender defender);
        List<IAttackModifier> BuffAttack(IAttackMethod method, IDefender defender);
        List<IDefenseModifier> DebuffDefense(IDefenseMethod method, IAttacker defender);
    }

    public interface IDefender
    {
        IDefenseMethod GetDefenseMethod(IAttackMethod method);
        List<IAttackModifier> DebuffAttack(IAttackMethod method, IAttacker attacker);
        List<IDefenseModifier> BuffDefense(IDefenseMethod method, IAttacker attacker);
    }

    public interface IAttackMethod
    {
        List<IAttackEffect> AttackEffects { get; set; }
        List<IAttackModifier> AttackModifiers { get; set; }
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
