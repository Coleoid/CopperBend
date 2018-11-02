using System;

namespace CopperBend.MapUtil
{
    public struct Point : IEquatable<Point>
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        #region Operators

        public static Point operator +(Point value1, Point value2)
        {
            return new Point(value1.X + value2.X, value1.Y + value2.Y);
        }

        public static Point operator -(Point value1, Point value2)
        {
            return new Point(value1.X - value2.X, value1.Y - value2.Y);
        }

        public static bool operator ==(Point a, Point b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Point a, Point b)
        {
            return !a.Equals(b);
        }

        #endregion

        #region Public methods

        public override bool Equals(object obj)
        {
            return obj is Point && Equals((Point)obj);
        }

        public bool Equals(Point other)
        {
            return (X == other.X) && (Y == other.Y);
        }

        public override int GetHashCode()
        {
            return X ^ Y;
        }

        public override string ToString()
        {
            return "{X:" + X + " Y:" + Y + "}";
        }

        public static float Distance(Point value1, Point value2)
        {
            float v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
            return (float)Math.Sqrt(v1 * v1 + v2 * v2);
        }

        public static Point Negate(Point value)
        {
            value.X = -value.X;
            value.Y = -value.Y;
            return value;
        }

        public double DistanceTo(Point point)
        {
            var span = this - point;
            return Math.Sqrt(span.X * span.X + span.Y * span.Y);
        }

        #endregion
    }
}
