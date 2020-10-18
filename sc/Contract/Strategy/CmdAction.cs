#pragma warning disable CA2227 // YAML s'zn wants collection setters
namespace CopperBend.Contract
{
    public enum CmdAction
    {
        Unset = 0,
        Incomplete,
        Unknown,
        Consume,
        Direction,
        Drop,
        PickUp,
        Throw,
        Use,
        Wait,
        Wield,
    }
}
#pragma warning restore CA2227 // YAML s'zn wants collection setters
