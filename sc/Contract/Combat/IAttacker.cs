using System.Collections.Generic;

namespace CopperBend.Contract
{
    //  This should be enough for much of the basic structure.
    //  Remaining questions:
    //  Does the RNG come from the attacker?  Is it supplied by the AttackSystem?
    //  Do the individual IAttacker/IDefenders decide which effects to apply, or do
    // they simply send their whole list?
    //  I expect we want the combo/filter logic outside of the combatants.

    public interface IAttacker
    {
        IAttackMethod GetAttackMethod(IDefender defender);
        List<IModifier> GetAttackModifiers(IDefender defender, IAttackMethod method);
    }
}
