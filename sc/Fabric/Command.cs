using CopperBend.Contract;

namespace CopperBend.Fabric
{
    public struct Command
    {
        public Command(CmdAction action, CmdDirection direction, Item item = null)
        {
            Action = action;
            Direction = direction;
            Item = item;
        }
        public CmdAction Action { get; }
        public CmdDirection Direction { get; }
        public Item Item { get; set; }

        public override string ToString()
        {
            var itemStr = Item == null ? string.Empty : " " + Item.Name;
            var dirStr = Direction == CmdDirection.None ? string.Empty : " in " + Direction.ToString();
            return $"{Action}{itemStr}{dirStr}";
        }
    }
}
