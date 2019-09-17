using GoRogue;
using System.Collections.Generic;

namespace CopperBend.Contract
{
    public interface IAreaBlight : IHasID, IDestroyable
    {
        int Extent { get; set; }
    }

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

    public class AttackMethod : IAttackMethod
    {
        public List<IAttackEffect> AttackEffects { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public List<IAttackModifier> AttackModifiers { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public AttackMethod()
        {
            AttackEffects = new List<IAttackEffect>();
            AttackModifiers = new List<IAttackModifier>();
        }
    }

    public interface IDefenseMethod
    {
    }

    public interface IAttackEffect
    {
        DamageType DamageType { get; set; }
        string DamageRange { get; set; }
    }

    public class AttackEffect : IAttackEffect
    {
        public DamageType DamageType { get; set; }
        public string DamageRange { get; set; }

        public AttackEffect()
        {
        }
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


    public class Demo_attdef_construction
    {
        public void some()
        {
            // Brekka-onu's flame hammer
            var bfh = new AttackMethod();
            var impact = new AttackEffect
            {
                DamageType = DamageType.Impact_blunt,
                DamageRange = "2d5 + 2"
            };
            var flame = new AttackEffect
            {
                DamageType = DamageType.Fire,
                DamageRange = "1d2 + 2"
            };
            bfh.AttackEffects.Add(impact);
            bfh.AttackEffects.Add(flame);
        }
    }
}
