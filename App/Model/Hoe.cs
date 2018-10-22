﻿namespace CopperBend.App.Model
{
    public class Hoe : Item
    {
        public override string Name { get => "Hoe"; }

        public Hoe(int x, int y)
            : base(x, y, 1, true)
        {
        }

        public override void ApplyTo(ITile tile, IControlPanel controls)
        {
            if (tile.IsTillable)
            {
                if (tile.IsTilled)
                {
                    controls.WriteLine("Already tilled.");
                    return;
                }

                tile.Till();
                controls.SetMapDirty();
                controls.PlayerBusyFor(15);
            }
            else
            {
                controls.WriteLine($"Cannot hoe {tile.TileType}.");
            }
        }
    }
}
