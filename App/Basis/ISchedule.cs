namespace CopperBend.App
{
    public interface ISchedule
    {
        int CurrentTick { get; }

        void Add(ScheduleEntry entry);
        void Clear();
        void DoNext(IControlPanel controls);
        ScheduleEntry GetNext();
        void Remove(ScheduleEntry entry);
        void RemoveActor(IActor targetActor);
    }
}