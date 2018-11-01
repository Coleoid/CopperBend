using CopperBend.MapUtil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace CopperBend.App
{
    public interface IMapCreationStrategy<T> where T : IMap
    {
        T CreateMap();
    }


    public class Map : IMap
    {
        private FieldOfView _fieldOfView;
        private bool[,] _isTransparent;
        private bool[,] _isWalkable;
        private bool[,] _isExplored;

        public Map()
        {
        }

        public Map(int width, int height)
        {
            Initialize(width, height);
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public void Initialize(int width, int height)
        {
            Width = width;
            Height = height;
            _isTransparent = new bool[width, height];
            _isWalkable = new bool[width, height];
            _isExplored = new bool[width, height];
            _fieldOfView = new FieldOfView(this);
        }

        public bool IsTransparent(int x, int y) => _isTransparent[x, y];

        public bool IsTransparent(Point point) => _isTransparent[point.X, point.Y];

        public bool IsWalkable(int x, int y) => _isWalkable[x, y];

        public bool IsWalkable(Point point) => _isWalkable[point.X, point.Y];

        public bool IsInFov(Point point) => _fieldOfView.IsInFov(point);

        public bool IsWithinMap(Point point)
        {
            return
                0 <= point.X && point.X < Width &&
                0 <= point.Y && point.Y < Height;
        }

        public bool IsExplored(int x, int y) => _isExplored[x, y];
        public bool IsExplored(Point point) =>_isExplored[point.X, point.Y];

        public void SetIsExplored(Point point, bool isExplored)
        {
            _isExplored[point.X, point.Y] = isExplored;
        }

        public void SetIsWalkable(Point point, bool isWalkable)
        {
            _isWalkable[point.X, point.Y] = isWalkable;
        }

        public void SetIsTransparent(Point point, bool isTransparent)
        {
            _isTransparent[point.X, point.Y] = isTransparent;
        }

        public Cell GetCellAt(Point point)
        {
            return GetCell(point.X, point.Y);
        }

        public void Clear()
        {
            Clear(true, true);
        }

        public void Clear(bool isTransparent, bool isWalkable)
        {
            foreach (Cell cell in GetAllCells())
            {
                SetIsTransparent(cell.Point, isTransparent);
                SetIsWalkable(cell.Point, isWalkable);
            }
        }

        public IMap Clone()
        {
            var map = new Map(Width, Height);
            foreach (Cell cell in GetAllCells())
            {
                map.CopyCellToMapLocation(cell);
            }

            return map;
        }

        public void CopyCellToMapLocation(Cell cell) => CopyCellToMapLocation(cell, cell.Point);

        public void CopyCellToMapLocation(Cell cell, Point point)
        {
            SetIsTransparent(point, cell.IsTransparent);
            SetIsWalkable(point, cell.IsWalkable);
            SetIsExplored(point, cell.IsExplored);
        }


        public void Copy(IMap sourceMap, int left, int top)
        {
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
                CopyCellToMapLocation(cell, new Point(cell.Point.X + left, cell.Point.Y + top));
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

        public IEnumerable<Cell> GetCellsInCircle(int xCenter, int yCenter, int radius)
        {
            var discovered = new HashSet<int>();

            int d = (5 - radius * 4) / 4;
            int x = 0;
            int y = radius;

            do
            {
                foreach (Cell cell in GetCellsAlongLine(xCenter + x, yCenter + y, xCenter - x, yCenter + y))
                {
                    if (AddToHashSet(discovered, cell))
                    {
                        yield return cell;
                    }
                }

                foreach (Cell cell in GetCellsAlongLine(xCenter - x, yCenter - y, xCenter + x, yCenter - y))
                {
                    if (AddToHashSet(discovered, cell))
                    {
                        yield return cell;
                    }
                }

                foreach (Cell cell in GetCellsAlongLine(xCenter + y, yCenter + x, xCenter - y, yCenter + x))
                {
                    if (AddToHashSet(discovered, cell))
                    {
                        yield return cell;
                    }
                }

                foreach (Cell cell in GetCellsAlongLine(xCenter + y, yCenter - x, xCenter - y, yCenter - x))
                {
                    if (AddToHashSet(discovered, cell))
                    {
                        yield return cell;
                    }
                }

                if (d < 0)
                {
                    d += 2 * x + 1;
                }
                else
                {
                    d += 2 * (x - y) + 1;
                    y--;
                }

                x++;
            } while (x <= y);
        }

        public IEnumerable<Cell> GetCellsInDiamond(int xCenter, int yCenter, int distance)
        {
            var discovered = new HashSet<int>();

            int xMin = Math.Max(0, xCenter - distance);
            int xMax = Math.Min(Width - 1, xCenter + distance);
            int yMin = Math.Max(0, yCenter - distance);
            int yMax = Math.Min(Height - 1, yCenter + distance);

            for (int i = 0; i <= distance; i++)
            {
                for (int j = distance; j >= 0 + i; j--)
                {
                    Cell cell;
                    if (AddToHashSet(discovered, Math.Max(xMin, xCenter - i), Math.Min(yMax, yCenter + distance - j),
                        out cell))
                    {
                        yield return cell;
                    }

                    if (AddToHashSet(discovered, Math.Max(xMin, xCenter - i), Math.Max(yMin, yCenter - distance + j),
                        out cell))
                    {
                        yield return cell;
                    }

                    if (AddToHashSet(discovered, Math.Min(xMax, xCenter + i), Math.Min(yMax, yCenter + distance - j),
                        out cell))
                    {
                        yield return cell;
                    }

                    if (AddToHashSet(discovered, Math.Min(xMax, xCenter + i), Math.Max(yMin, yCenter - distance + j),
                        out cell))
                    {
                        yield return cell;
                    }
                }
            }
        }

        public IEnumerable<Point> GetCoordsInSquare(int xCenter, int yCenter, int distance)
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


        public IEnumerable<Cell> GetCellsInSquare(int xCenter, int yCenter, int distance)
        {
            int xMin = Math.Max(0, xCenter - distance);
            int xMax = Math.Min(Width - 1, xCenter + distance);
            int yMin = Math.Max(0, yCenter - distance);
            int yMax = Math.Min(Height - 1, yCenter + distance);

            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    yield return GetCell(x, y);
                }
            }
        }

        public IEnumerable<Cell> GetBorderCellsInCircle(int xCenter, int yCenter, int radius)
        {
            var discovered = new HashSet<int>();

            int d = (5 - radius * 4) / 4;
            int x = 0;
            int y = radius;

            Cell centerCell = GetCell(xCenter, yCenter);

            do
            {
                Cell cell;
                if (AddToHashSet(discovered, ClampX(xCenter + x), ClampY(yCenter + y), centerCell, out cell))
                {
                    yield return cell;
                }

                if (AddToHashSet(discovered, ClampX(xCenter + x), ClampY(yCenter - y), centerCell, out cell))
                {
                    yield return cell;
                }

                if (AddToHashSet(discovered, ClampX(xCenter - x), ClampY(yCenter + y), centerCell, out cell))
                {
                    yield return cell;
                }

                if (AddToHashSet(discovered, ClampX(xCenter - x), ClampY(yCenter - y), centerCell, out cell))
                {
                    yield return cell;
                }

                if (AddToHashSet(discovered, ClampX(xCenter + y), ClampY(yCenter + x), centerCell, out cell))
                {
                    yield return cell;
                }

                if (AddToHashSet(discovered, ClampX(xCenter + y), ClampY(yCenter - x), centerCell, out cell))
                {
                    yield return cell;
                }

                if (AddToHashSet(discovered, ClampX(xCenter - y), ClampY(yCenter + x), centerCell, out cell))
                {
                    yield return cell;
                }

                if (AddToHashSet(discovered, ClampX(xCenter - y), ClampY(yCenter - x), centerCell, out cell))
                {
                    yield return cell;
                }

                if (d < 0)
                {
                    d += 2 * x + 1;
                }
                else
                {
                    d += 2 * (x - y) + 1;
                    y--;
                }

                x++;
            } while (x <= y);
        }

        public IEnumerable<Cell> GetBorderCellsInDiamond(int xCenter, int yCenter, int distance)
        {
            var discovered = new HashSet<int>();

            int xMin = Math.Max(0, xCenter - distance);
            int xMax = Math.Min(Width - 1, xCenter + distance);
            int yMin = Math.Max(0, yCenter - distance);
            int yMax = Math.Min(Height - 1, yCenter + distance);

            Cell centerCell = GetCell(xCenter, yCenter);
            Cell cell;
            if (AddToHashSet(discovered, xCenter, yMin, centerCell, out cell))
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

        public IEnumerable<Cell> GetCellsInRows(params int[] rowNumbers)
        {
            foreach (int y in rowNumbers)
            {
                for (int x = 0; x < Width; x++)
                {
                    yield return GetCell(x, y);
                }
            }
        }

        public IEnumerable<Cell> GetCellsInColumns(params int[] columnNumbers)
        {
            foreach (int x in columnNumbers)
            {
                for (int y = 0; y < Height; y++)
                {
                    yield return GetCell(x, y);
                }
            }
        }

        public Cell GetCell(int x, int y)
        {
            return new Cell(x, y, _isTransparent[x, y], _isWalkable[x, y], _fieldOfView.IsInFov(x, y),
                _isExplored[x, y]);
        }

        public Cell GetCell(Point point) => GetCell(point.X, point.Y);

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

        public MapState Save()
        {
            var mapState = new MapState();
            mapState.Width = Width;
            mapState.Height = Height;
            mapState.Cells = new MapState.CellProperties[Width * Height];
            foreach (Cell cell in GetAllCells())
            {
                var cellProperties = MapState.CellProperties.None;
                if (cell.IsInFov)
                {
                    cellProperties |= MapState.CellProperties.Visible;
                }

                if (cell.IsTransparent)
                {
                    cellProperties |= MapState.CellProperties.Transparent;
                }

                if (cell.IsWalkable)
                {
                    cellProperties |= MapState.CellProperties.Walkable;
                }

                mapState.Cells[cell.Point.Y * Width + cell.Point.X] = cellProperties;
            }

            return mapState;
        }

        public void Restore(MapState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state", "Map state cannot be null");
            }

            var inFov = new HashSet<int>();

            Initialize(state.Width, state.Height);
            foreach (Cell cell in GetAllCells())
            {
                MapState.CellProperties cellProperties = state.Cells[cell.Point.Y * Width + cell.Point.X];
                if (cellProperties.HasFlag(MapState.CellProperties.Visible))
                {
                    inFov.Add(IndexFor(cell.Point));
                }

                _isTransparent[cell.Point.X, cell.Point.Y] =
                    cellProperties.HasFlag(MapState.CellProperties.Transparent);
                _isWalkable[cell.Point.X, cell.Point.Y] = cellProperties.HasFlag(MapState.CellProperties.Walkable);
            }

            _fieldOfView = new FieldOfView(this, inFov);
        }

        public static Map Create(IMapCreationStrategy<Map> mapCreationStrategy)
        {
            if (mapCreationStrategy == null)
            {
                throw new ArgumentNullException("mapCreationStrategy", "Map creation strategy cannot be null");
            }

            return mapCreationStrategy.CreateMap();
        }

        public Cell CellFor(int index)
        {
            int x = index % Width;
            int y = index / Width;

            return GetCell(x, y);
        }

        public int IndexFor(int x, int y)
        {
            return (y * Width) + x;
        }

        public int IndexFor(Point cell)
        {
            return (cell.Y * Width) + cell.X;
        }

        private bool AddToHashSet(HashSet<int> hashSet, int x, int y, out Cell cell)
        {
            cell = GetCell(x, y);
            return hashSet.Add(IndexFor(cell.Point));
        }

        private bool AddToHashSet(HashSet<int> hashSet, int x, int y, Cell centerCell, out Cell cell)
        {
            cell = GetCell(x, y);
            if (cell.Equals(centerCell)) return false;
            return hashSet.Add(IndexFor(cell.Point));
        }

        private bool AddToHashSet(HashSet<int> hashSet, Cell cell)
        {
            return hashSet.Add(IndexFor(cell.Point));
        }

        public override string ToString()
        {
            return ToString(false);
        }
    }
}
