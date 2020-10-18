using System;

namespace CopperBend.Contract
{
    /// <summary> Flags of the different ways an event can be triggered </summary>
    [Flags]
    public enum TriggerCategories
    {
        Unset = 0,
        PlayerLocation = 1,
        PlayerProximity = 2,
        PlayerLineOfSight = 4,
        Mortality = 8,
        WorldState = 16,
        TriggerCompleted = 32,
        MapChanged = 64,
        //  et c.
    }
}
