using System;

namespace CopperBend.Contract
{
    public interface ISchedule
    {
        int CurrentTick { get; }

        void Add(Action<IControlPanel> action, int offset);
        void AddActor(IActor actor, int offset);
        void Clear();
        Action<IControlPanel> GetNextAction();
    }
}
