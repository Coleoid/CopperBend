using GoRogue;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CbRework
{
    // Extends the SadConsole.Entities.Entity class
    // by adding an ID to it using GoRogue's ID system
    public abstract class CbEntity : SadConsole.Entities.Entity, GoRogue.IHasID
    {
        // one IDGenerator for all Entities
        public static IDGenerator IDGenerator = new IDGenerator();

        public uint ID { get; private set; }

        protected CbEntity(Color foreground, Color background, int glyph, int width = 1, int height = 1) 
            : base(width, height)
        {
            Animation.CurrentFrame[0].Foreground = foreground;
            Animation.CurrentFrame[0].Background = background;
            Animation.CurrentFrame[0].Glyph = glyph;

            ID = IDGenerator.UseID();
        }
    }

    public abstract class Actor : CbEntity
    {
        public int Health { get; set; }
        public int MaxHealth { get; set; }

        protected Actor(Color foreground, Color background, int glyph, int width = 1, int height = 1)
            : base(foreground, background, glyph, width, height)
        {
        }
    }

    public class Player : Actor
    {
        public Player(Color foreground, Color background, int glyph = '@', int width = 1, int height = 1)
            : base(foreground, background, glyph, width, height)
        {
        }
    }

    public class Monster : Actor
    {
        public Monster(Color foreground, Color background) 
            : base(foreground, background, 'M')
        {
        }
    }
}
