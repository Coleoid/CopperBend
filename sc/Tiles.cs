using SadConsole;
using Microsoft.Xna.Framework;

namespace CbRework
{
    public abstract class TileBase : Cell
    {
        public string Name;
        public bool AllowsLOS;
        public bool AllowsMove;

        ///<summary>Adds Name, AllowsLOS, and AllowsMove to SadConsole.Cell.</summary>
        public TileBase(
            Color foreground, Color background, int glyph, 
            string name = "", bool allowsLOS = true, bool allowsMove = true)
            : base(foreground, background, glyph)
        {
            AllowsLOS = allowsLOS;
            AllowsMove = allowsMove;
            Name = name;
        }

        public TileBase() : base()
        {
        }
    }

    public class TileWall : TileBase
    {
        public TileWall(string name = "wall", bool allowsLOS = false, bool allowsMove = false, char glyph = '#')
            : base(Color.LightGray, Color.Transparent, glyph, name, allowsLOS, allowsMove)
        {
        }
    }

    public class TileFloor : TileBase
    {
        public TileFloor(string name = "floor", bool allowsLOS = true, bool allowsMove = true, char glyph = '.')
            : base(Color.DarkGray, Color.Transparent, glyph, name, allowsLOS, allowsMove)
        {
        }
    }
}
