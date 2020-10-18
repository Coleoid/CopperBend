using System.Collections.Generic;

namespace CopperBend.Contract
{
    public interface IDefender : IDelible
    {
        IDefenseMethod GetDefenseMethod(IAttackMethod method);
        List<IModifier> GetDefenseModifiers(IAttacker attacker, IAttackMethod method);
    }
}
