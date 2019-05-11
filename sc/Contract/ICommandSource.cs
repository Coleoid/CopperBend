using System;

namespace CopperBend.Contract
{
    public interface ICommandSource
    {
        void GiveCommand(IBeing being);
    }

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
