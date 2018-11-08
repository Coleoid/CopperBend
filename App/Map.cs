using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace CopperBend.MapUtil
{
    public class Map : IMap
    {
        public Map(int width, int height)
        {
            Initialize(width, height);
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        private bool[,] _isTransparent;
        private bool[,] _isWalkable;
        private bool[,] _isExplored;
        private FieldOfView _fieldOfView;

        public void Initialize(int width, int height)
        {
            Width = width;
            Height = height;
            _isTransparent = new bool[width, height];
            _isWalkable = new bool[width, height];
            _isExplored = new bool[width, height];
            _fieldOfView = new FieldOfView(this);
        }

        public void Clear()
        {
            Clear(true, true);
        }

        public void Clear(bool isTransparent, bool isWalkable)
        {
            foreach (Point point in GetAllPoints())
            {
                SetIsTransparent(point, isTransparent);
                SetIsWalkable(point, isWalkable);
            }
        }

        public bool IsTransparent(int x, int y) => _isTransparent[x, y];
        public bool IsTransparent(Point point) => _isTransparent[point.X, point.Y];
        public void SetIsTransparent(Point point, bool isTransparent)
        {
            _isTransparent[point.X, point.Y] = isTransparent;
        }

        public bool IsWalkable(Point point) => _isWalkable[point.X, point.Y];
        public void SetIsWalkable(Point point, bool isWalkable)
        {
            _isWalkable[point.X, point.Y] = isWalkable;
        }

        public bool IsExplored(Point point) => _isExplored[point.X, point.Y];
        public void SetIsExplored(Point point, bool isExplored)
        {
            _isExplored[point.X, point.Y] = isExplored;
        }

        public bool IsInFov(Point point) => _fieldOfView.IsInFov(point);

        public bool IsWithinMap(Point point)
        {
            return
                0 <= point.X && point.X < Width &&
                0 <= point.Y && point.Y < Height;
        }

        public IMap Clone()
        {
            var map = new Map(Width, Height);
            foreach (Cell cell in GetAllCells())
            {
                map.CopyCellToLocation(cell);
            }

            return map;
        }

        public void CopyCellToLocation(Cell cell) => CopyCellToLocation(cell, cell.Point);
        public void CopyCellToLocation(Cell cell, Point point)
        {
            SetIsTransparent(point, cell.IsTransparent);
            SetIsWalkable(point, cell.IsWalkable);
            SetIsExplored(point, cell.IsExplored);
        }

        public void Copy(IMap sourceMap, Point offset)
        {
            if (sourceMap == null)
            {
                throw new ArgumentNullException("sourceMap", "Source map cannot be null");
            }

            if (sourceMap.Width + offset.X > Width)
            {
                throw new ArgumentException(
                    "Source map width + left offset cannot be larger than the destination map width");
            }

            if (sourceMap.Height + offset.Y > Height)
            {
                throw new ArgumentException(
                    "Source map height + top offset cannot be larger than the destination map height");
            }

            foreach (Cell cell in sourceMap.GetAllCells())
            {
                CopyCellToLocation(cell, cell.Point + offset);
            }
        }

        public ReadOnlyCollection<Cell> ComputeFov(Point origin, int radius, bool lightWalls)
        {
            return _fieldOfView.ComputeFov(origin, radius, lightWalls);
        }

        public ReadOnlyCollection<Cell> AppendFov(Point origin, int radius, bool lightWalls)
        {
            return _fieldOfView.AppendFov(origin, radius, lightWalls);
        }

        public IEnumerable<Cell> GetAllCells()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    yield return GetCell(x, y);
                }
            }
        }

        public IEnumerable<Point> GetAllPoints()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    yield return new Point(x, y);
                }
            }
        }

        private int ClampX(int x)
        {
            return (x < 0) ? 0 : (x > Width - 1) ? Width - 1 : x;
        }

        private int ClampY(int y)
        {
            return (y < 0) ? 0 : (y > Height - 1) ? Height - 1 : y;
        }

        private Point NearestPointInsideMapTo(Point source)
        {
            return new Point(ClampX(source.X), ClampY(source.Y));
        }

        private bool IsOutsideMap(int x, int y)
        {
            return x < 0 || y < 0
                || x >= Width
                || y >= Height;
        }

        public IEnumerable<Cell> GetCellsAlongLine(Point origin, Point destination)
        {
            var span = destination - origin;
            int dx = Math.Abs(span.X);
            int dy = Math.Abs(span.Y);
            int err = dx - dy;

            int sx = origin.X < destination.X ? 1 : -1;
            int sy = origin.Y < destination.Y ? 1 : -1;

            int nextX = origin.X;
            int nextY = origin.Y;
            if (!IsOutsideMap(nextX, nextY))
            {
                yield return GetCell(nextX, nextY);
            }

            while (true)
            {
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err = err - dy;
                    nextX += sx;
                }

                if (e2 < dx)
                {
                    err = err + dx;
                    nextY += sy;
                }

                if (IsOutsideMap(nextX, nextY)) continue;
                yield return GetCell(nextX, nextY);

                if (destination.X == nextX && destination.Y == nextY) break;
            }
        }


        public IEnumerable<Point> GetPointsInSquare(Point center, int distance)
        {
            int xMin = Math.Max(0, center.X - distance);
            int xMax = Math.Min(Width - 1, center.X + distance);
            int yMin = Math.Max(0, center.Y - distance);
            int yMax = Math.Min(Height - 1, center.Y + distance);

            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    yield return new Point(x, y);
                }
            }
        }

        public IEnumerable<Cell> GetBorderCellsInDiamond(Point center, int distance)
        {
            var discovered = new HashSet<int>();
            //center = NearestPointInsideMapTo(center);
            int xMin = Math.Max(0, center.X - distance);
            int xMax = Math.Min(Width - 1, center.X + distance);
            int yMin = Math.Max(0, center.Y - distance);
            int yMax = Math.Min(Height - 1, center.Y + distance);

            Cell centerCell = GetCell(center);
            if (AddToHashSet(discovered, center.X, yMin, centerCell, out Cell cell))
            {
                yield return cell;
            }

            if (AddToHashSet(discovered, center.X, yMax, centerCell, out cell))
            {
                yield return cell;
            }

            for (int i = 1; i <= distance; i++)
            {
                if (AddToHashSet(discovered, Math.Max(xMin, center.X - i), Math.Min(yMax, center.Y + distance - i),
                    centerCell, out cell))
                {
                    yield return cell;
                }

                if (AddToHashSet(discovered, Math.Max(xMin, center.X - i), Math.Max(yMin, center.Y - distance + i),
                    centerCell, out cell))
                {
                    yield return cell;
                }

                if (AddToHashSet(discovered, Math.Min(xMax, center.X + i), Math.Min(yMax, center.Y + distance - i),
                    centerCell, out cell))
                {
                    yield return cell;
                }

                if (AddToHashSet(discovered, Math.Min(xMax, center.X + i), Math.Max(yMin, center.Y - distance + i),
                    centerCell, out cell))
                {
                    yield return cell;
                }
            }
        }

        public IEnumerable<Cell> GetBorderCellsInSquare(Point center, int distance)
        {
            int xMin = Math.Max(0, center.X - distance);
            int xMax = Math.Min(Width - 1, center.X + distance);
            int yMin = Math.Max(0, center.Y - distance);
            int yMax = Math.Min(Height - 1, center.Y + distance);
            List<Cell> borderCells = new List<Cell>();

            for (int x = xMin; x <= xMax; x++)
            {
                borderCells.Add(GetCell(x, yMin));
                borderCells.Add(GetCell(x, yMax));
            }

            for (int y = yMin + 1; y <= yMax - 1; y++)
            {
                borderCells.Add(GetCell(xMin, y));
                borderCells.Add(GetCell(xMax, y));
            }

            Cell centerCell = GetCell(center.X, center.Y);
            borderCells.Remove(centerCell);

            return borderCells;
        }

        public string ToString(bool useFov)
        {
            var mapRepresentation = new StringBuilder();
            int lastY = 0;
            foreach (Cell cell in GetAllCells())
            {
                if (cell.Point.Y != lastY)
                {
                    lastY = cell.Point.Y;
                    mapRepresentation.Append(Environment.NewLine);
                }

                mapRepresentation.Append(cell.ToString(useFov));
            }

            return mapRepresentation.ToString().TrimEnd('\r', '\n');
        }

        public Cell GetCell(int index)
        {
            int x = index % Width;
            int y = index / Width;

            return GetCell(x, y);
        }

        public Cell GetCell(int x, int y)
        {
            return new Cell(x, y, _isTransparent[x, y], _isWalkable[x, y], _fieldOfView.IsInFov(x, y),
                _isExplored[x, y]);
        }

        public Cell GetCell(Point point) => GetCell(point.X, point.Y);

        public int GetIndex(int x, int y)
        {
            return (y * Width) + x;
        }

        public int GetIndex(Point cell)
        {
            return (cell.Y * Width) + cell.X;
        }

        public Point GetPoint(int index)
        {
            int x = index % Width;
            int y = index / Width;
            return new Point(x, y);
        }

        private bool AddToHashSet(HashSet<int> hashSet, int x, int y, Cell centerCell, out Cell cell)
        {
            cell = GetCell(x, y);
            if (cell.Equals(centerCell)) return false;
            return hashSet.Add(GetIndex(cell.Point));
        }

        public override string ToString()
        {
            return ToString(false);
        }
    }
}
