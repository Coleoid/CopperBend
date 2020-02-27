using System.Collections.Generic;
using System.Linq;
using CopperBend.Contract;

namespace CopperBend.Model.Aspects
{

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
