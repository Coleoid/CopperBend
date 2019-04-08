//using System;

using Microsoft.Xna.Framework;

namespace CbRework
{
    public class Map
    {
        public TileBase[] Tiles { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Point PlayerStartPoint { get; set; }

        //Build a new map with a specified width and height
        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new TileBase[width * height];
        }

        public bool IsTileWalkable(Point location)
        {
            // off the map is disallowed
            if (location.X < 0 || location.X >= Width
             || location.Y < 0 || location.Y >= Height)
                return false;

            return Tiles[location.Y * Width + location.X].AllowsMove;
        }
    }
}
