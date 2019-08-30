using System;
using System.Collections.Generic;
using GoRogue;

namespace CopperBend.Fabric
{
    public class Guard
    {
        public static void Against(bool condition, string message = "Condition violated")
        {
            if (condition) throw new Exception(message);
        }

        public static void AgainstNullArgument(object argument, string message = "Argument null")
        {
            if (argument == null) throw new Exception(message);
        }

    }

    public static class CoordExtensions
    {
        public static IEnumerable<Coord> Neighbors(this Coord self)
        {
            for (int y = self.Y - 1; y < self.Y + 2; y++)
            for (int x = self.X - 1; x < self.X + 2; x++)
            {
                if (x == self.X && y == self.Y) continue;
                yield return new Coord(x, y);
            }
        }
    }
}
