using System;

namespace CopperBend.Contract
{
    [Flags]
    public enum DescMods
    {
        None = 0,
        Article = 1,
        Definite = 2,
        Quantity = 4,
        LeadingCapital = 8,
        NoAdjective = 16,
    }
}
