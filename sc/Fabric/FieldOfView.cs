using CopperBend.Contract;
using Microsoft.Xna.Framework;
using SadConsole;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CopperBend.Fabric
{
    public class FieldOfView
    {
        private readonly IMap _map;
        private readonly HashSet<int> _inFov;

        public FieldOfView(IMap map)
        {
            _map = map;
            _inFov = new HashSet<int>();
        }

        //public FieldOfView(IMap map, HashSet<int> inFov)
        //{
        //    _map = map;
        //    _inFov = inFov;
        //}

        public ReadOnlyCollection<Cell> ComputeFov(Point origin, int radius, bool lightWalls)
        {
            ClearFov();
            return AppendFov(origin, radius, lightWalls);
        }

        public ReadOnlyCollection<Cell> AppendFov(Point origin, int radius, bool lightWalls)
        {
            foreach (Cell borderCell in _map.GetBorderCellsInSquare(origin, radius))
            {
                foreach (Cell cell in _map.GetCellsAlongLine(origin, borderCell.Point))
                {
                    //var distance = origin.DistanceTo(cell.Point);
                    var span = origin - cell.Point;
                    var distance = Math.Sqrt(span.X * span.X + span.Y * span.Y);
                    if (distance - .5 > radius) break;

                    if (cell.IsTransparent)
                    {
                        _inFov.Add(_map.GetIndex(cell.Point));
                    }
                    else
                    {
                        if (lightWalls)
                        {
                            _inFov.Add(_map.GetIndex(cell.Point));
                        }
                        break;
                    }
                }
            }

            if (lightWalls)
            {
                // Post processing step created based on the algorithm at this website:
                // https://sites.google.com/site/jicenospam/visibilitydetermination
                foreach (Point coord in _map.GetPointsInSquare(origin, radius))
                {
                    if (coord.X > origin.X)
                    {
                        if (coord.Y > origin.Y)
                        {
                            PostProcessFovQuadrant(coord, Quadrant.SE);
                        }
                        else if (coord.Y < origin.Y)
                        {
                            PostProcessFovQuadrant(coord, Quadrant.NE);
                        }
                    }
                    else if (coord.X < origin.X)
                    {
                        if (coord.Y > origin.Y)
                        {
                            PostProcessFovQuadrant(coord, Quadrant.SW);
                        }
                        else if (coord.Y < origin.Y)
                        {
                            PostProcessFovQuadrant(coord, Quadrant.NW);
                        }
                    }
                }
            }

            return CellsInFov();
        }

        public bool IsInFov(int x, int y) => _inFov.Contains(_map.GetIndex(x, y));
        public bool IsInFov(Point point) => _inFov.Contains(_map.GetIndex(point));

        private ReadOnlyCollection<Cell> CellsInFov()
        {
            var cells = new List<Cell>();
            foreach (int index in _inFov)
            {
                cells.Add(_map.GetCell(index));
            }
            return new ReadOnlyCollection<Cell>(cells);
        }

        private void ClearFov()
        {
            _inFov.Clear();
        }

        private void PostProcessFovQuadrant(Point point, Quadrant quadrant)
        {
            int x1 = point.X;
            int y1 = point.Y;
            int x2 = point.X;
            int y2 = point.Y;
            switch (quadrant)
            {
            case Quadrant.NE:
                {
                    y1 = point.Y + 1;
                    x2 = point.X - 1;
                    break;
                }
            case Quadrant.SE:
                {
                    y1 = point.Y - 1;
                    x2 = point.X - 1;
                    break;
                }
            case Quadrant.SW:
                {
                    y1 = point.Y - 1;
                    x2 = point.X + 1;
                    break;
                }
            case Quadrant.NW:
                {
                    y1 = point.Y + 1;
                    x2 = point.X + 1;
                    break;
                }
            }
            if (!IsInFov(point) && !_map.IsTransparent(point))
            {
                if ((_map.IsTransparent(x1, y1) && IsInFov(x1, y1)) ||
                    (_map.IsTransparent(x2, y2) && IsInFov(x2, y2)) || 
                    (_map.IsTransparent(x2, y1) && IsInFov(x2, y1)))
                {
                    _inFov.Add(_map.GetIndex(point));
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
