using System;
using System.Collections.Generic;
using System.Linq;

namespace CopperBend.MapUtil
{
    public class PathFinder
    {
        private readonly EdgeWeightedDigraph _graph;
        private readonly IMap _map;

        public PathFinder(IMap map)
        {
            _map = map ?? throw new ArgumentNullException("map", "Map cannot be null");
            _graph = new EdgeWeightedDigraph(_map.Width * _map.Height);

            foreach (Cell cell in _map.GetAllCells())
            {
                if (cell.IsWalkable)
                {
                    int v = IndexFor(cell.Point);
                    foreach (Cell neighbor in _map.GetBorderCellsInDiamond(cell.Point, 1))
                    {
                        if (neighbor.IsWalkable)
                        {
                            int w = IndexFor(neighbor.Point);
                            _graph.AddEdge(new DirectedEdge(v, w, 1.0));
                            _graph.AddEdge(new DirectedEdge(w, v, 1.0));
                        }
                    }
                }
            }
        }

        public PathFinder(IMap map, double diagonalCost)
        {
            _map = map ?? throw new ArgumentNullException("map", "Map cannot be null");
            _graph = new EdgeWeightedDigraph(_map.Width * _map.Height);

            foreach (Cell cell in _map.GetAllCells())
            {
                if (cell.IsWalkable)
                {
                    int v = IndexFor(cell.Point);
                    foreach (Cell neighbor in _map.GetBorderCellsInSquare(cell.Point, 1))
                    {
                        if (neighbor.IsWalkable)
                        {
                            int w = IndexFor(neighbor.Point);
                            if (neighbor.Point.X != cell.Point.X && neighbor.Point.Y != cell.Point.Y)
                            {
                                _graph.AddEdge(new DirectedEdge(v, w, diagonalCost));
                                _graph.AddEdge(new DirectedEdge(w, v, diagonalCost));
                            }
                            else
                            {
                                _graph.AddEdge(new DirectedEdge(v, w, 1.0));
                                _graph.AddEdge(new DirectedEdge(w, v, 1.0));
                            }
                        }
                    }
                }
            }
        }

        public PathFinder(IMap map, double cardinalCost, double diagonalCost)
        {
            _map = map ?? throw new ArgumentNullException("map");
            _graph = new EdgeWeightedDigraph(_map.Width * _map.Height);

            foreach (Cell cell in _map.GetAllCells())
            {
                if (!cell.IsWalkable) continue;

                int cellIndex = IndexFor(cell.Point);
                var neighborCoords = new List<(Point, double)>
            {
               (new Point(cell.Point.X + 0, cell.Point.Y - 1), cardinalCost),
               (new Point(cell.Point.X + 0, cell.Point.Y + 1), cardinalCost),
               (new Point(cell.Point.X + 1, cell.Point.Y + 0), cardinalCost),
               (new Point(cell.Point.X - 1, cell.Point.Y + 0), cardinalCost),
               (new Point(cell.Point.X - 1, cell.Point.Y - 1), diagonalCost),
               (new Point(cell.Point.X - 1, cell.Point.Y + 1), diagonalCost),
               (new Point(cell.Point.X + 1, cell.Point.Y - 1), diagonalCost),
               (new Point(cell.Point.X + 1, cell.Point.Y + 1), diagonalCost),
            };

                foreach (var (coord, cost) in neighborCoords)
                {
                    if (!map.IsWithinMap(coord)) continue;
                    if (!map.IsWalkable(coord)) continue;

                    int neighborIndex = IndexFor(coord);
                    _graph.AddEdge(new DirectedEdge(cellIndex, neighborIndex, cost));
                }
            }
        }

        public Path ShortestPath(Cell source, Cell destination)
        {
            Path shortestPath = TryFindShortestPath(source, destination);

            if (shortestPath == null)
            {
                throw new Exception(string.Format("Path from ({0}, {1}) to ({2}, {3}) not found", source.Point.X, source.Point.Y, destination.Point.X, destination.Point.Y));
            }

            return shortestPath;
        }

        public Path TryFindShortestPath(Cell source, Cell destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }

            var cells = ShortestPathCells(source, destination).ToList();
            if (cells[0] == null)
            {
                return null;
            }
            return new Path(cells);
        }

        public List<Point> ShortestPathList(Point source, Point destination)
        {
            var list = new List<Point>();
            IEnumerable<DirectedEdge> path = DijkstraShortestPath
               .FindPath(_graph, IndexFor(source), IndexFor(destination));

            if (path == null) return list;

            foreach (DirectedEdge edge in path)
            {
                list.Add(CoordFor(edge.To));
            }

            return list;
        }

        private IEnumerable<Cell> ShortestPathCells(Cell source, Cell destination)
        {
            IEnumerable<DirectedEdge> path = DijkstraShortestPath
               .FindPath(_graph, IndexFor(source.Point), IndexFor(destination.Point));
            if (path == null) yield break;

            yield return source;
            foreach (DirectedEdge edge in path)
            {
                yield return CellFor(edge.To);
            }
        }

        private int IndexFor(Point cell)
        {
            return (cell.Y * _map.Width) + cell.X;
        }

        private Point CoordFor(int index)
        {
            int x = index % _map.Width;
            int y = index / _map.Width;

            return new Point(x, y);
        }

        private Cell CellFor(int index)
        {
            int x = index % _map.Width;
            int y = index / _map.Width;

            return _map.GetCell(x, y);
        }
    }
}
