namespace CopperBend.Contract
{
    public struct Command
    {
        public Command(CmdAction action, CmdDirection direction, IItem item = null, IUsable usable = null)
        {
            Action = action;
            Direction = direction;
            Item = item;
            Usable = usable;
        }
        public CmdAction Action { get; }
        public CmdDirection Direction { get; }
        public IItem Item { get; set; }
        public IUsable Usable { get; set; }

        public override string ToString()
        {
            var itemStr = Item == null ? string.Empty : " " + Item.Name;
            var dirStr = Direction == CmdDirection.None ? string.Empty : " in " + Direction.ToString();
            return $"{Action}{itemStr}{dirStr}";
        }
    }
}
