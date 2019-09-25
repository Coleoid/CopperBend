using System.Collections.Generic;
using CopperBend.Contract;

namespace CopperBend.Model
{
    public class AttackMethod : IAttackMethod
    {
        public List<IAttackEffect> AttackEffects { get; set; }
        public List<IModifier> AttackModifiers { get; set; }

        public AttackMethod()
        {
            AttackEffects = new List<IAttackEffect>();
            AttackModifiers = new List<IModifier>();
        }
    }

    public class DefenseMethod : IDefenseMethod
    {
        public Dictionary<DamageType, string> DamageResistances { get; set; }

        public DefenseMethod()
        {
            DamageResistances = new Dictionary<DamageType, string>();
        }
    }

    public class AttackEffect : IAttackEffect
    {
        public DamageType DamageType { get; set; }
        public string DamageRange { get; set; }

        public AttackEffect()
        {
        }
    }

    public class AttackDamage
    {
        public DamageType Type { get; set; }
        public int Initial { get; set; }
        public int Current { get; set; }
    }

}
