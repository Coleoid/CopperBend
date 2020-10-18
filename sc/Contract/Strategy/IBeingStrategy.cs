using System.Collections.Generic;

#pragma warning disable CA2227 // YAML s'zn wants collection setters
namespace CopperBend.Contract
{
    /// <summary> A BeingStrategy supplies a being's action decisions </summary>
    public interface IBeingStrategy
    {
        void GiveCommand(IBeing being);
        string SubType { get; }
    }
}
#pragma warning restore CA2227 // YAML s'zn wants collection setters
