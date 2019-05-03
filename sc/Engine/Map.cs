using Microsoft.Xna.Framework;
using GoRogue;
using CopperBend.Contract;

namespace CopperBend.Engine
{
    public class SpaceMap : SpatialMap<Space>
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Point PlayerStartPoint { get; set; }

        public SpaceMap(int width, int height)
            : base(height * width)
        {
            Width = width;
            Height = height;
        }

        public bool CanWalkThrough(Point location)
        {
            // off the map is not walkable
            if (location.X < 0 || location.X >= Width
             || location.Y < 0 || location.Y >= Height)
                return false;

            return GetItem(location).CanWalkThrough;
        }
    }
}
