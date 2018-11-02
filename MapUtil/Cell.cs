using System;

namespace CopperBend.MapUtil
{
    public struct Cell : IEquatable<Cell>
    {
        /// <summary> Where on the map we're describing </summary>
        public Point Point { get; private set; }

        /// <summary> Will a Field of View pass through this location? </summary>
        public bool IsTransparent { get; private set; }

        /// <summary> Can a normal traveler pass through this location? </summary>
        public bool IsWalkable { get; private set; }

        /// <summary> Is this location in the latest FOV? </summary>
        public bool IsInFov { get; private set; }

        /// <summary> Has this location ever been seen by the player? </summary>
        public bool IsExplored { get; private set; }

        public Cell(int x, int y, bool isTransparent, bool isWalkable, bool isInFov, bool isExplored)
        {
            Point = new Point(x, y);
            IsTransparent = isTransparent;
            IsWalkable = isWalkable;
            IsInFov = isInFov;
            IsExplored = isExplored;
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool useFov)
        {
            if (useFov && !IsInFov) return "%";

            if (IsWalkable)
            {
                return IsTransparent ? "." : "~";
            }
            else
            {
                return IsTransparent ? "_" : "#";
            }
        }

        public override bool Equals(object obj)
        {
            return obj != null
                   && obj is Cell
                   && Equals((Cell)obj);
        }

        public bool Equals(Cell other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Point.Equals(other.Point)
               && IsTransparent == other.IsTransparent
               && IsWalkable == other.IsWalkable
               && IsInFov == other.IsInFov
               && IsExplored == other.IsExplored;
        }

        public static bool operator ==(Cell left, Cell right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Cell left, Cell right)
        {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Point.X;
                hashCode = (hashCode * 397) ^ Point.Y;
                hashCode = (hashCode * 397) ^ IsTransparent.GetHashCode();
                hashCode = (hashCode * 397) ^ IsWalkable.GetHashCode();
                hashCode = (hashCode * 397) ^ IsInFov.GetHashCode();
                hashCode = (hashCode * 397) ^ IsExplored.GetHashCode();
                return hashCode;
            }
        }
    }
}
