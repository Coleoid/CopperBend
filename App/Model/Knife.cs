using CopperBend.MapUtil;
using System;

namespace CopperBend.App.Model
{
    public class Knife : Item
    {
        public override string Name { get => "knife"; }

        public Knife(Point point)
            : base(point, 1, true)
        {
        }

        public override void ApplyTo(ITile tile, IControlPanel controls, Direction direction)
        {
            throw new NotImplementedException();
        }
    }
}
