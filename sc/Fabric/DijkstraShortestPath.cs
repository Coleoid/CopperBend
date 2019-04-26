using System;
using System.Collections.Generic;

namespace CopperBend.Fabric
{
    public class DijkstraShortestPath
    {
        private readonly double[] _distanceTo;
        private readonly DirectedEdge[] _edgeTo;
        private readonly IndexMinPriorityQueue<double> _priorityQueue;

        public DijkstraShortestPath(EdgeWeightedDigraph graph, int sourceVertex)
           : this(graph, sourceVertex, null)
        {
        }

        private DijkstraShortestPath(EdgeWeightedDigraph graph, int sourceVertex, int? destinationVertex)
        {
            if (graph == null)
            {
                throw new ArgumentNullException("graph", "EdgeWeightedDigraph cannot be null");
            }

            foreach (DirectedEdge edge in graph.Edges())
            {
                if (edge.Weight < 0)
                {
                    throw new ArgumentOutOfRangeException(string.Format("Edge: '{0}' has negative weight", edge));
                }
            }

            _distanceTo = new double[graph.NumberOfVertices];
            _edgeTo = new DirectedEdge[graph.NumberOfVertices];
            for (int v = 0; v < graph.NumberOfVertices; v++)
            {
                _distanceTo[v] = Double.PositiveInfinity;
            }
            _distanceTo[sourceVertex] = 0.0;

            _priorityQueue = new IndexMinPriorityQueue<double>(graph.NumberOfVertices);
            _priorityQueue.Insert(sourceVertex, _distanceTo[sourceVertex]);
            while (!_priorityQueue.IsEmpty())
            {
                int v = _priorityQueue.DeleteMin();

                if (destinationVertex.HasValue && v == destinationVertex.Value)
                {
                    return;
                }

                foreach (DirectedEdge edge in graph.Adjacent(v))
                {
                    Relax(edge);
                }
            }
        }

        public static IEnumerable<DirectedEdge> FindPath(EdgeWeightedDigraph graph, int sourceVertex, int destinationVertex)
        {
            var dijkstraShortestPath = new DijkstraShortestPath(graph, sourceVertex, destinationVertex);
            return dijkstraShortestPath.PathTo(destinationVertex);
        }

        private void Relax(DirectedEdge edge)
        {
            int v = edge.From;
            int w = edge.To;
            if (_distanceTo[w] > _distanceTo[v] + edge.Weight)
            {
                _distanceTo[w] = _distanceTo[v] + edge.Weight;
                _edgeTo[w] = edge;
                if (_priorityQueue.Contains(w))
                {
                    _priorityQueue.DecreaseKey(w, _distanceTo[w]);
                }
                else
                {
                    _priorityQueue.Insert(w, _distanceTo[w]);
                }
            }
        }

        public double DistanceTo(int destinationVertex)
        {
            return _distanceTo[destinationVertex];
        }

        public bool HasPathTo(int destinationVertex)
        {
            return _distanceTo[destinationVertex] < double.PositiveInfinity;
        }

        public IEnumerable<DirectedEdge> PathTo(int destinationVertex)
        {
            if (!HasPathTo(destinationVertex))
            {
                return null;
            }
            var path = new Stack<DirectedEdge>();
            for (DirectedEdge edge = _edgeTo[destinationVertex]; edge != null; edge = _edgeTo[edge.From])
            {
                path.Push(edge);
            }
            return path;
        }

        public bool Check(EdgeWeightedDigraph graph, int sourceVertex)
        {
            if (graph == null)
            {
                throw new ArgumentNullException("graph", "EdgeWeightedDigraph cannot be null");
            }

            if (_distanceTo[sourceVertex] != 0.0 || _edgeTo[sourceVertex] != null)
            {
                return false;
            }
            for (int v = 0; v < graph.NumberOfVertices; v++)
            {
                if (v == sourceVertex)
                {
                    continue;
                }
                if (_edgeTo[v] == null && _distanceTo[v] != double.PositiveInfinity)
                {
                    return false;
                }
            }
            for (int v = 0; v < graph.NumberOfVertices; v++)
            {
                foreach (DirectedEdge edge in graph.Adjacent(v))
                {
                    int w = edge.To;
                    if (_distanceTo[v] + edge.Weight < _distanceTo[w])
                    {
                        return false;
                    }
                }
            }
            for (int w = 0; w < graph.NumberOfVertices; w++)
            {
                if (_edgeTo[w] == null)
                {
                    continue;
                }
                DirectedEdge edge = _edgeTo[w];
                int v = edge.From;
                if (w != edge.To)
                {
                    return false;
                }
                if (_distanceTo[v] + edge.Weight != _distanceTo[w])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
