using System;
using System.Collections.Generic;

namespace CopperBend.App.Model
{
    public class DefenseAspect : IDefenseAspect
    {
        public IActor Actor { get; set; }
        private Dictionary<string, double> Resistances { get; set; } = new Dictionary<string, double>();

        public DefenseAspect()
        {
            Resistances["General"] = 0;
        }

        public void SetResistance(string damageType, double portionResisted)
        {
            Resistances[damageType] = portionResisted;
        }

        public int ApplyDamage(IDamage damage)
        {
            double portionBlocked = 0;
            if (Resistances.ContainsKey(damage.DamageType))
            {
                portionBlocked = Resistances[damage.DamageType];
            }
            else
            {
                portionBlocked = Resistances["General"];
            }

            var blocked = (int)Math.Ceiling(portionBlocked * damage.Quantity);
            var done = damage.Quantity - blocked;
            Actor.Hurt(done);
            return done;
        }
    }
}
