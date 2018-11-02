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

        public bool IsWalkable(int x, int y) => _isWalkable[x, y];
        public bool IsWalkable(Point point) => _isWalkable[point.X, point.Y];
        public void SetIsWalkable(Point point, bool isWalkable)
        {
            _isWalkable[point.X, point.Y] = isWalkable;
        }

        public bool IsExplored(int x, int y) => _isExplored[x, y];
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

        public void Copy(IMap sourceMap, int left, int top)
        {
            Point offset = new Point(left, top);
            if (sourceMap == null)
            {
                throw new ArgumentNullException("sourceMap", "Source map cannot be null");
            }

            if (sourceMap.Width + left > Width)
            {
                throw new ArgumentException(
                    "Source map 'width' + 'left' cannot be larger than the destination map width");
            }

            if (sourceMap.Height + top > Height)
            {
                throw new ArgumentException(
                    "Source map 'height' + 'top' cannot be larger than the destination map height");
            }

            foreach (Cell cell in sourceMap.GetAllCells())
            {
                CopyCellToLocation(cell, cell.Point + offset);
            }
        }

        public ReadOnlyCollection<Cell> ComputeFov(int xOrigin, int yOrigin, int radius, bool lightWalls)
        {
            return _fieldOfView.ComputeFov(xOrigin, yOrigin, radius, lightWalls);
        }

        public ReadOnlyCollection<Cell> AppendFov(int xOrigin, int yOrigin, int radius, bool lightWalls)
        {
            return _fieldOfView.AppendFov(xOrigin, yOrigin, radius, lightWalls);
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

        public IEnumerable<Cell> GetCellsAlongLine(int xOrigin, int yOrigin, int xDestination, int yDestination)
        {
            xOrigin = ClampX(xOrigin);
            yOrigin = ClampY(yOrigin);
            xDestination = ClampX(xDestination);
            yDestination = ClampY(yDestination);

            int dx = Math.Abs(xDestination - xOrigin);
            int dy = Math.Abs(yDestination - yOrigin);

            int sx = xOrigin < xDestination ? 1 : -1;
            int sy = yOrigin < yDestination ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                yield return GetCell(xOrigin, yOrigin);
                if (xOrigin == xDestination && yOrigin == yDestination)
                {
                    break;
                }

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err = err - dy;
                    xOrigin = xOrigin + sx;
                }

                if (e2 < dx)
                {
                    err = err + dx;
                    yOrigin = yOrigin + sy;
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


        public IEnumerable<Point> GetPointsInSquare(int xCenter, int yCenter, int distance)
        {
            int xMin = Math.Max(0, xCenter - distance);
            int xMax = Math.Min(Width - 1, xCenter + distance);
            int yMin = Math.Max(0, yCenter - distance);
            int yMax = Math.Min(Height - 1, yCenter + distance);

            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    yield return new Point(x, y);
                }
            }
        }

        public IEnumerable<Cell> GetBorderCellsInDiamond(int xCenter, int yCenter, int distance)
        {
            var discovered = new HashSet<int>();

            int xMin = Math.Max(0, xCenter - distance);
            int xMax = Math.Min(Width - 1, xCenter + distance);
            int yMin = Math.Max(0, yCenter - distance);
            int yMax = Math.Min(Height - 1, yCenter + distance);

            Cell centerCell = GetCell(xCenter, yCenter);
            if (AddToHashSet(discovered, xCenter, yMin, centerCell, out Cell cell))
            {
                yield return cell;
            }

            if (AddToHashSet(discovered, xCenter, yMax, centerCell, out cell))
            {
                yield return cell;
            }

            for (int i = 1; i <= distance; i++)
            {
                if (AddToHashSet(discovered, Math.Max(xMin, xCenter - i), Math.Min(yMax, yCenter + distance - i),
                    centerCell, out cell))
                {
                    yield return cell;
                }

                if (AddToHashSet(discovered, Math.Max(xMin, xCenter - i), Math.Max(yMin, yCenter - distance + i),
                    centerCell, out cell))
                {
                    yield return cell;
                }

                if (AddToHashSet(discovered, Math.Min(xMax, xCenter + i), Math.Min(yMax, yCenter + distance - i),
                    centerCell, out cell))
                {
                    yield return cell;
                }

                if (AddToHashSet(discovered, Math.Min(xMax, xCenter + i), Math.Max(yMin, yCenter - distance + i),
                    centerCell, out cell))
                {
                    yield return cell;
                }
            }
        }

        public IEnumerable<Cell> GetBorderCellsInSquare(int xCenter, int yCenter, int distance)
        {
            int xMin = Math.Max(0, xCenter - distance);
            int xMax = Math.Min(Width - 1, xCenter + distance);
            int yMin = Math.Max(0, yCenter - distance);
            int yMax = Math.Min(Height - 1, yCenter + distance);
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

            Cell centerCell = GetCell(xCenter, yCenter);
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
