using System;
using System.Collections.Generic;
using System.Text;

namespace CopperBend.MapUtil
{
    public class Path
    {
        private readonly LinkedList<Cell> _steps;
        private LinkedListNode<Cell> _currentStep;

        public Path(IEnumerable<Cell> steps)
        {
            _steps = new LinkedList<Cell>(steps);

            if (_steps.Count < 1)
            {
                throw new ArgumentException("Path must have steps", "steps");
            }

            _currentStep = _steps.First;
        }

        public Cell Start
        {
            get
            {
                return _steps.First.Value;
            }
        }

        public Cell End
        {
            get
            {
                return _steps.Last.Value;
            }
        }

        public int Length
        {
            get
            {
                return _steps.Count;
            }
        }

        public Cell CurrentStep
        {
            get
            {
                return _currentStep.Value;
            }
        }

        public IEnumerable<Cell> Steps
        {
            get
            {
                return _steps;
            }
        }

        public Cell StepForward()
        {
            Cell cell = TryStepForward();

            if (cell == null)
            {
                throw new Exception("Cannot take a step foward when at the end of the path");
            }

            return cell;
        }

        public Cell TryStepForward()
        {
            LinkedListNode<Cell> nextStep = _currentStep.Next;
            if (nextStep == null)
            {
                throw new Exception("No next step");
            }
            _currentStep = nextStep;
            return nextStep.Value;
        }

        public Cell StepBackward()
        {
            Cell cell = TryStepBackward();

            if (cell == null)
            {
                throw new Exception("Cannot take a step backward when at the start of the path");
            }

            return cell;
        }

        public Cell TryStepBackward()
        {
            LinkedListNode<Cell> previousStep = _currentStep.Previous;
            if (previousStep == null)
            {
                throw new Exception("No next step");
            }
            _currentStep = previousStep;
            return previousStep.Value;
        }
    }

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
