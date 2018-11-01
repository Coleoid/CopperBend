﻿using CopperBend.MapUtil;

namespace CopperBend.App.Model
{
    public class Hoe : Item
    {
        public override string Name { get => "Hoe"; }

        public Hoe(Point point)
            : base(point, 1, true)
        {
        }

        public override void ApplyTo(ITile tile, IControlPanel controls, Direction direction)
        {
            if (tile.IsTillable)
            {
                if (tile.IsTilled)
                {
                    controls.WriteLine("Already tilled.");
                    return;
                }

                controls.Till(tile);
                controls.SetMapDirty();
                controls.PlayerBusyFor(15);
            }
            else
            {
                controls.WriteLine($"Cannot hoe {tile.TileType.Name}.");
            }
        }
    }
}
