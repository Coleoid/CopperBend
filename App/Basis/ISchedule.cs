using System;

namespace CopperBend.App
{
    public interface ISchedule
    {
        int CurrentTick { get; }

        void Add(Action<IControlPanel> action, int offset);
        void Clear();
        void DoNext(IControlPanel controls);
        Action<IControlPanel> GetNextAction();
    }
}
