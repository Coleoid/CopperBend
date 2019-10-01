using System.Collections.Generic;

namespace CopperBend.Contract
{
    public interface ISchedule
    {
        int CurrentTick { get; }

        void AddEntry(ScheduleEntry entry);
        void Clear();
        ScheduleEntry GetNextAction();
        void AddAgent(IScheduleAgent agent);
        void AddAgent(IScheduleAgent agent, int offset);
        void RemoveAgent(IScheduleAgent agent);
    }

    public class ScheduleEntry
    {
        public IScheduleAgent Agent { get; set; }
        public ScheduleAction Action { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public int Offset { get; set; }
    }

    public enum ScheduleAction
    {
        Unset = 0,

        GetCommand,
        SeedGrows,
        PlantGrows,
    }
}
