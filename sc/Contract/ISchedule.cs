using System;

namespace CopperBend.Contract
{
    public interface ISchedule
    {
        int CurrentTick { get; }

        void AddEntry(ScheduleEntry entry);
        void Clear();
        Action<IControlPanel> GetNextAction();
        void AddAgent(IScheduleAgent agent);
        void AddAgent(IScheduleAgent agent, int offset);
    }

    public struct ScheduleEntry
    {
        public Action<IControlPanel> Action;
        public int Offset;
        public IScheduleAgent Agent;
    }
}
