using System;
using System.Collections.Generic;
using System.Text;

namespace CopperBend.Fabric
{
    public class EdgeWeightedDigraph
    {
        private readonly LinkedList<DirectedEdge>[] _adjacent;

        public EdgeWeightedDigraph(int vertices)
        {
            NumberOfVertices = vertices;
            NumberOfEdges = 0;
            _adjacent = new LinkedList<DirectedEdge>[NumberOfVertices];
            for (int v = 0; v < NumberOfVertices; v++)
            {
                _adjacent[v] = new LinkedList<DirectedEdge>();
            }
        }

        public int NumberOfVertices { get; private set; }

        public int NumberOfEdges { get; private set; }

        public void AddEdge(DirectedEdge edge)
        {
            if (edge == null)
            {
                throw new ArgumentNullException("edge", "DirectedEdge cannot be null");
            }

            _adjacent[edge.From].AddLast(edge);
        }

        public IEnumerable<DirectedEdge> Adjacent(int vertex)
        {
            return _adjacent[vertex];
        }

        public IEnumerable<DirectedEdge> Edges()
        {
            for (int v = 0; v < NumberOfVertices; v++)
            {
                foreach (DirectedEdge edge in _adjacent[v])
                {
                    yield return edge;
                }
            }
        }

        public int OutDegree(int vertex)
        {
            return _adjacent[vertex].Count;
        }

        public override string ToString()
        {
            var formattedString = new StringBuilder();
            formattedString.AppendFormat("{0} vertices, {1} edges {2}", NumberOfVertices, NumberOfEdges, Environment.NewLine);
            for (int v = 0; v < NumberOfVertices; v++)
            {
                formattedString.AppendFormat("{0}: ", v);
                foreach (DirectedEdge edge in _adjacent[v])
                {
                    formattedString.AppendFormat("{0} ", edge.To);
                }
                formattedString.AppendLine();
            }
            return formattedString.ToString();
        }
    }
}
