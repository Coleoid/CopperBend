using System.Collections.Generic;

namespace CopperBend.Contract
{
    public interface IDefenseMethod
    {
        Dictionary<string, string> Resistances { get; }
    }
}
