//using System;

using Microsoft.Xna.Framework;

namespace CbRework
{
    // Extends the SadConsole.Entities.Entity class
    // by adding an ID to it using GoRogue's ID system
    public abstract class CbEntity : SadConsole.Entities.Entity, GoRogue.IHasID
    {
        public uint ID { get; private set; } // stores the entity's unique identification number

        protected CbEntity(Color foreground, Color background, int glyph, int width = 1, int height = 1) : base(width, height)
        {
            Animation.CurrentFrame[0].Foreground = foreground;
            Animation.CurrentFrame[0].Background = background;
            Animation.CurrentFrame[0].Glyph = glyph;

            // Create a new unique identifier for this entity
            ID = Map.IDGenerator.UseID();
        }
    }

}
