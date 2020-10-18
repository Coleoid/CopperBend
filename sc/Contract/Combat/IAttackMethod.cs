using System.Collections.Generic;

namespace CopperBend.Contract
{
    public interface IAttackMethod
    {
        List<IAttackEffect> AttackEffects { get; }
        List<IModifier> AttackModifiers { get; }
        void AddEffect(IAttackEffect effect);
    }
}
