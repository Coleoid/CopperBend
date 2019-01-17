using System;
using System.Collections.Generic;

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

        public Cell Start => _steps.First.Value;

        public Cell End => _steps.Last.Value;

        public int Length => _steps.Count;

        public Cell CurrentStep => _currentStep.Value;

        public IEnumerable<Cell> Steps => _steps;

        public Cell StepForward()
        {
            LinkedListNode<Cell> nextStep = _currentStep.Next;
            _currentStep = nextStep ?? throw new Exception("Cannot take a step foward when at the end of the path");
            return nextStep.Value;
        }

        public Cell StepBackward()
        {
            LinkedListNode<Cell> previousStep = _currentStep.Previous;
            _currentStep = previousStep ?? throw new Exception("Cannot take a step backward when at the start of the path");
            return previousStep.Value;
        }
    }
}
