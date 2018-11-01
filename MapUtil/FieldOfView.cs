using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CopperBend.MapUtil
{
    public class FieldOfView
    {
        private readonly IMap _map;
        //_map.IndexFor(x, y)
        // _map.GetBorderCellsInSquare
        //_map.GetCellsAlongLine
        //_map.GetCoordsInSquare(
        //_map.CellFor(
        //_map.IsTransparent(


        private readonly HashSet<int> _inFov;

        public FieldOfView(IMap map)
        {
            _map = map;
            _inFov = new HashSet<int>();
        }

        public FieldOfView(IMap map, HashSet<int> inFov)
        {
            _map = map;
            _inFov = inFov;
        }

        public FieldOfView Clone()
        {
            var inFovCopy = new HashSet<int>();
            foreach (int i in _inFov)
            {
                inFovCopy.Add(i);
            }
            return new FieldOfView(_map, inFovCopy);
        }

        public ReadOnlyCollection<Cell> ComputeFov(int xOrigin, int yOrigin, int radius, bool lightWalls)
        {
            ClearFov();
            return AppendFov(xOrigin, yOrigin, radius, lightWalls);
        }

        public ReadOnlyCollection<Cell> AppendFov(int xOrigin, int yOrigin, int radius, bool lightWalls)
        {
            foreach (Cell borderCell in _map.GetBorderCellsInSquare(xOrigin, yOrigin, radius))
            {
                foreach (Cell cell in _map.GetCellsAlongLine(xOrigin, yOrigin, borderCell.Point.X, borderCell.Point.Y))
                {
                    var xDist = cell.Point.X - xOrigin;
                    var yDist = cell.Point.Y - yOrigin;
                    var dist = Math.Sqrt(xDist * xDist + yDist * yDist);
                    if (dist - .5 > radius) break;

                    if (cell.IsTransparent)
                    {
                        _inFov.Add(_map.IndexFor(cell.Point));
                    }
                    else
                    {
                        if (lightWalls)
                        {
                            _inFov.Add(_map.IndexFor(cell.Point));
                        }
                        break;
                    }
                }
            }

            if (lightWalls)
            {
                // Post processing step created based on the algorithm at this website:
                // https://sites.google.com/site/jicenospam/visibilitydetermination
                foreach (Point coord in _map.GetCoordsInSquare(xOrigin, yOrigin, radius))
                {
                    if (coord.X > xOrigin)
                    {
                        if (coord.Y > yOrigin)
                        {
                            PostProcessFovQuadrant(coord.X, coord.Y, Quadrant.SE);
                        }
                        else if (coord.Y < yOrigin)
                        {
                            PostProcessFovQuadrant(coord.X, coord.Y, Quadrant.NE);
                        }
                    }
                    else if (coord.X < xOrigin)
                    {
                        if (coord.Y > yOrigin)
                        {
                            PostProcessFovQuadrant(coord.X, coord.Y, Quadrant.SW);
                        }
                        else if (coord.Y < yOrigin)
                        {
                            PostProcessFovQuadrant(coord.X, coord.Y, Quadrant.NW);
                        }
                    }
                }
            }

            return CellsInFov();
        }

        public bool IsInFov(int x, int y) => _inFov.Contains(_map.IndexFor(x, y));
        public bool IsInFov(Point point) => _inFov.Contains(_map.IndexFor(point));

        private ReadOnlyCollection<Cell> CellsInFov()
        {
            var cells = new List<Cell>();
            foreach (int index in _inFov)
            {
                cells.Add(_map.CellFor(index));
            }
            return new ReadOnlyCollection<Cell>(cells);
        }

        private void ClearFov()
        {
            _inFov.Clear();
        }

        private void PostProcessFovQuadrant(int x, int y, Quadrant quadrant)
        {
            int x1 = x;
            int y1 = y;
            int x2 = x;
            int y2 = y;
            switch (quadrant)
            {
            case Quadrant.NE:
                {
                    y1 = y + 1;
                    x2 = x - 1;
                    break;
                }
            case Quadrant.SE:
                {
                    y1 = y - 1;
                    x2 = x - 1;
                    break;
                }
            case Quadrant.SW:
                {
                    y1 = y - 1;
                    x2 = x + 1;
                    break;
                }
            case Quadrant.NW:
                {
                    y1 = y + 1;
                    x2 = x + 1;
                    break;
                }
            }
            if (!IsInFov(x, y) && !_map.IsTransparent(x, y))
            {
                if ((_map.IsTransparent(x1, y1) && IsInFov(x1, y1)) || (_map.IsTransparent(x2, y2) && IsInFov(x2, y2))
                     || (_map.IsTransparent(x2, y1) && IsInFov(x2, y1)))
                {
                    _inFov.Add(_map.IndexFor(x, y));
                }
            }
        }

        private enum Quadrant
        {
            NE = 1,
            SE = 2,
            SW = 3,
            NW = 4
        }
    }
}
