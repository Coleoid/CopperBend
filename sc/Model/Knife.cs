using CopperBend.Contract;
using Microsoft.Xna.Framework;
using System;

namespace CopperBend.Model
{
    public class Knife : Item
    {
        public override string Name { get => "knife"; }

        public Knife(Point point)
            : base(point, 1, true)
        {
        }

        public override void ApplyTo(ITile tile, IControlPanel controls, IMessageOutput output, CmdDirection direction)
        {
            throw new NotImplementedException();
        }
    }
}
