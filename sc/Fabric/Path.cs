using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CopperBend.Fabric
{
    
    public class Path
    {
        private readonly LinkedList<Point> _steps;
        private LinkedListNode<Point> _currentStep;

        public Path(IEnumerable<Point> steps)
        {
            _steps = new LinkedList<Point>(steps);

            if (_steps.Count < 1)
            {
                throw new ArgumentException("Path must have steps", "steps");
            }

            _currentStep = _steps.First;
        }

        public Point Start => _steps.First.Value;

        public Point End => _steps.Last.Value;

        public int Length => _steps.Count;

        public Point CurrentStep => _currentStep.Value;

        public IEnumerable<Point> Steps => _steps;

        public Point StepForward()
        {
            LinkedListNode<Point> nextStep = _currentStep.Next;
            _currentStep = nextStep ?? throw new Exception("Cannot take a step foward when at the end of the path");
            return nextStep.Value;
        }

        public Point StepBackward()
        {
            LinkedListNode<Point> previousStep = _currentStep.Previous;
            _currentStep = previousStep ?? throw new Exception("Cannot take a step backward when at the start of the path");
            return previousStep.Value;
        }
    }
}
