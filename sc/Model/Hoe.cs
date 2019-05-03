﻿using CopperBend.Contract;
using Microsoft.Xna.Framework;

namespace CopperBend.Model
{
    public class Hoe : Item
    {
        public override string Name { get => "hoe"; }

        public Hoe(Point point)
            : base(point, 1, true)
        {
        }

        //public override void ApplyTo(Space space, IControlPanel controls, IMessageOutput output, CmdDirection direction)
        //{
        //    if (space.IsTillable)
        //    {
        //        if (space.IsTilled)
        //        {
        //            output.WriteLine("Already tilled.");
        //            return;
        //        }

        //        controls.Till(space);
        //        //controls.SetMapDirty();
        //        //controls.ScheduleActor(15);
        //    }
        //    else
        //    {
        //        output.WriteLine($"Cannot till the {space.Terrain.Name}.");
        //    }
        //}
    }
}
