using System;
using GoRogue;
using CopperBend.Contract;

namespace CopperBend.Model
{
    public class Knife : Item
    {
        public override string Name { get => "knife"; }
        public override string ItemType { get => "Knife"; }

        public Knife(Coord position, int quantity = 1, bool isUsable = true, uint id = uint.MaxValue)
            : base(position, quantity, isUsable, id)
        {
        }

        public override void ApplyTo(Coord position, IControlPanel controls, ILogWindow output, CmdDirection direction)
        {
            throw new NotImplementedException();
        }
    }
}
