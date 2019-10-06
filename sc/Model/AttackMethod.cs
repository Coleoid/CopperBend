using System.Collections.Generic;
using CopperBend.Contract;

namespace CopperBend.Model
{
    public class AttackMethod : IAttackMethod
    {
        public List<IAttackEffect> AttackEffects { get; set; }
        public List<IModifier> AttackModifiers { get; set; }

        public AttackMethod(string type, string range)
            : this()
        {
            AddEffect(type, range);
        }

        public AttackMethod()
        {
            AttackEffects = new List<IAttackEffect>();
            AttackModifiers = new List<IModifier>();
        }

        public void AddEffect(string type, string range)
        {
            AddEffect(new AttackEffect {
                Type = type,
                DamageRange = range
            });
        }

        public void AddEffect(IAttackEffect effect) => AttackEffects.Add(effect);

        public void AddModifier(IModifier modifier) => AttackModifiers.Add(modifier);
    }

    public class DefenseMethod : IDefenseMethod
    {
        public Dictionary<string, string> Resistances { get; set; }

        public DefenseMethod()
        {
            Resistances = new Dictionary<string, string>();
        }
    }

    public class AttackEffect : IAttackEffect
    {
        public string Type { get; set; }
        public string DamageRange { get; set; }
    }

    public class AttackDamage
    {
        public AttackDamage(int initial, string type)
        {
            Initial = Current = initial;
            Type = type;
        }

        public string Type { get; set; }
        public int Initial { get; set; }
        public int Current { get; set; }
    }

}
