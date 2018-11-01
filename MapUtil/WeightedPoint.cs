using System;

namespace CopperBend.MapUtil
{
    public struct WeightedPoint : IEquatable<WeightedPoint>
    {
        private Point _point;

        public int Weight { get; set; }

        public int X
        {
            get { return _point.X; }
            set { _point = new Point(value, _point.Y); }
        }

        public int Y
        {
            get { return _point.Y; }
            set { _point = new Point(_point.X, value); }
        }

        public bool Equals(WeightedPoint other)
        {
            return X == other.X && Y == other.Y && Weight == other.Weight;
        }
    }
}
