using System;

namespace CopperBend.Contract
{
    [Flags]
    public enum UseTargetFlags
    {
        Unset = 0,
        Self = 1,
        Being = 2,
        Direction = 4,
        Location = 8,
        Item = 16,
    }
}
