using System;

namespace CopperBend.App
{
    public interface ICommandSource
    {
        //Command GetCommand();
        void GiveCommand(IActor actor);
    }

    public enum CmdAction
    {
        Unset = 0,
        None,
        Move,
        Unknown,
        PickUp,
        Consume,
        Drop,
        Use,
        Wait,
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

    public struct Command
    {
        public Command(CmdAction action, CmdDirection direction, IItem item = null)
        {
            Action = action;
            Direction = direction;
            Item = item;
        }
        public CmdAction Action { get; }
        public CmdDirection Direction { get; }
        public IItem Item { get; set; }

        public override string ToString()
        {
            var itemStr = Item == null ? string.Empty : " " + Item.Name;
            var dirStr = Direction == CmdDirection.None ? string.Empty : " in " + Direction.ToString();
            return $"{Action}{itemStr}{dirStr}";
        }
    }
}
