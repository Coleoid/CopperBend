using System;

#pragma warning disable CA2227 // YAML s'zn wants collection setters
namespace CopperBend.Contract
{
    [Flags]
    public enum CmdDirection
    {
        None = 0,
        North = 1,
        East = 2,
        South = 4,
        West = 8,
        Up = 16,
        Down = 32,
        Northeast = 3,
        Southeast = 6,
        Southwest = 12,
        Northwest = 9,
    }
}
#pragma warning restore CA2227 // YAML s'zn wants collection setters
