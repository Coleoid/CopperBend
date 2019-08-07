using System;
using GoRogue;
using CopperBend.Contract;

namespace CopperBend.Model
{
    public class Knife : Item
    {
        public override string Name { get => "knife"; }
        public override string ItemType { get => "Knife"; }

        public Knife(Coord position)
            : base(position, 1, true)
        {
        }

        public override void ApplyTo(Coord position, IControlPanel controls, ILogWindow output, CmdDirection direction)
        {
            throw new NotImplementedException();
        }
    }
}
